# Postilio for .NET

The official .NET client for the [Postilio](https://postilio.eu) API: European transactional email.

| Package | What |
|---|---|
| `Postilio.Client` | The API client: send email, read messages, manage domains, suppressions and webhooks, verify webhook signatures. `services.AddPostilio(...)` for dependency injection. |
| `Postilio.Client.AspNetCore` | Sends ASP.NET Core Identity's email (confirmation links, password resets) through Postilio. |

Both target .NET 8 and .NET 10, are trimming- and native-AOT-compatible, and depend on nothing but
`Microsoft.Extensions.Http` (and, for the second, ASP.NET Core).

> Not published on NuGet yet; the API is in alpha. See [CHANGELOG.md](CHANGELOG.md).

## Install

```sh
dotnet add package Postilio.Client
```

## Quickstart

Create an API key in the portal under **Keys & SMTP**. A test key (`pk_test_…`) goes through every check but never
delivers, so start with one. The sender must be on a verified domain of the key's project.

```csharp
using Postilio;

var postilio = new PostilioClient(Environment.GetEnvironmentVariable("POSTILIO_KEY") ?? throw new InvalidOperationException("Set POSTILIO_KEY."));

var sent = await postilio.SendEmailAsync(new SendEmailRequest
{
    From = "Acme <no-reply@mail.example.com>",
    To = ["delivered@simulator.postilio.eu"],
    Subject = "Welcome to Acme",
    Text = "Hi Ada, your account is ready.",
    Html = "<p>Hi Ada, your account is ready.</p>",
    Tag = "welcome",
});

var email = await postilio.GetEmailAsync(sent.Ids[0]);
Console.WriteLine(email.Status); // "delivered": a test key simulates it at once
```

Create one `PostilioClient` and reuse it; it is thread-safe. Every method takes a `CancellationToken`.

## With dependency injection

```json
// appsettings.json: keep the key itself in user secrets or your secret store, not in the file
{
  "Postilio": {
    "ApiKey": "",
    "MaxRetries": 2,
    "MaxRetryDelay": "00:00:30"
  }
}
```

```csharp
builder.Services.AddPostilio(builder.Configuration.GetSection("Postilio"));
// or: builder.Services.AddPostilio(o => o.ApiKey = builder.Configuration["Postilio:ApiKey"] ?? string.Empty);

app.MapPost("/signup", async (PostilioClient postilio, CancellationToken ct) => { /* … */ });
```

`AddPostilio` registers `PostilioClient` as a typed client of `IHttpClientFactory`, checks the settings when the app
starts, and redacts the `Authorization` header from the factory's logs: the key is never logged. A reloaded
configuration section, such as a rotated key, applies to clients resolved after the reload. It returns the
`IHttpClientBuilder`, for your own handlers or a timeout (see [Retries](#retries)).

## Sending safely twice: idempotency

A request can time out after Postilio accepted it. `SendEmailAsync` therefore always sends an `Idempotency-Key`: one it
makes per call, so its own retries never send twice. Pass your own key to be safe across restarts and queues too:

```csharp
await postilio.SendEmailAsync(receipt, $"order-{order.Id}-receipt", ct);
```

Within 24 hours, the same key with the same request answers as the first time and sends nothing; the same key with
another request throws a `PostilioConflictException` (`idempotency_key_reused_with_different_request`).

## Errors

An error answer throws a `PostilioException`, or a subclass per status:

| Status | Exception |
|---|---|
| 400 | `PostilioValidationException`, with `Errors` per field |
| 401 | `PostilioAuthenticationException` |
| 403 | `PostilioPermissionException` |
| 404 | `PostilioNotFoundException` |
| 409 | `PostilioConflictException` |
| 422 | `PostilioUnprocessableException` |
| 429 | `PostilioRateLimitException`, with `RetryAfter` |
| 5xx | `PostilioServerException`, with the `TraceId` to quote to support |

`ErrorCode` holds the API's stable code; `PostilioErrorCodes` lists them. A code may be added later, so handle one you
do not know by its status:

```csharp
try
{
    await postilio.SendEmailAsync(message, ct);
}
catch (PostilioUnprocessableException e) when (e.ErrorCode == PostilioErrorCodes.UnverifiedSenderDomain)
{
    // the sender's domain is not verified (yet)
}
catch (PostilioRateLimitException e)
{
    // e.g. the sandbox's daily limit: try again after e.RetryAfter
}
```

## Retries

The client retries, at most `MaxRetries` times (2 by default):

- a **429** for every call, after the `Retry-After` Postilio sends. If that is longer than `MaxRetryDelay` (30 seconds by
  default), it throws at once with `RetryAfter` set, rather than blocking your request for an hour;
- a **connection failure** or a **408, 500, 502, 503 or 504** only when sending again cannot do anything twice: a `GET`,
  or a send, which carries an `Idempotency-Key`. Creating, changing and deleting are never retried on these.

Waits grow from half a second, with some random spread, and never exceed `MaxRetryDelay`. Set `MaxRetries = 0` to turn
retries off, for instance when you prefer your own resilience handler; do not add one on top of these retries
(`AddStandardResilienceHandler` would also retry creating and deleting).

`HttpClient.Timeout` (100 seconds by default) covers a whole call, its retries and waits included, and throws a
`TaskCanceledException` when it runs out. Set it with `AddPostilio(...).ConfigureHttpClient(c => c.Timeout = ...)`.

## Domains, suppressions and webhooks

These need a live key with the matching scope (`domains:manage`, `suppressions:manage`, `webhooks:manage`).

```csharp
var domain = await postilio.CreateDomainAsync(new CreateDomainRequest { Name = "mail.example.com" });
foreach (var record in domain.Records)
{
    Console.WriteLine($"{record.Type} {record.Name} → {record.Value}");
}
await postilio.CheckDomainAsync(domain.Id); // once DNS is in place

var page = await postilio.ListSuppressionsAsync(reason: SuppressionReasons.HardBounce, limit: 100);
while (page.Next is { } next)
{
    page = await postilio.ListSuppressionsAsync(reason: SuppressionReasons.HardBounce, before: next, limit: 100);
}

var created = await postilio.CreateWebhookEndpointAsync(new CreateWebhookEndpointRequest
{
    Url = "https://api.example.com/hooks/postilio",
    Events = [WebhookEventTypes.Delivered, WebhookEventTypes.Bounced, WebhookEventTypes.Complained],
});
// created.Secret (whsec_…) is shown this once: store it in your secret store now.
```

Statuses, reasons and event types are strings with constants (`EmailStatuses`, `DomainStatuses`, …), since Postilio may
add values.

## Receiving webhooks

Verify every delivery before you trust it. `WebhookVerifier` implements the
[Standard Webhooks](https://www.standardwebhooks.com/) scheme: a constant-time comparison, a timestamp within five
minutes either way (so a captured request cannot be replayed later), and both signatures during a secret rotation.
Verify the raw body, before parsing it.

```csharp
using Postilio.Webhooks;

var verifier = new WebhookVerifier(builder.Configuration["Postilio:WebhookSecret"] ?? string.Empty);

app.MapPost("/hooks/postilio", async (HttpRequest request, CancellationToken ct) =>
{
    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync(ct);
    var h = request.Headers;
    if (!verifier.Verify(h["webhook-id"], h["webhook-timestamp"], h["webhook-signature"], body))
    {
        return Results.Unauthorized();
    }
    var webhook = WebhookEvent.Parse(body);
    // Deliveries are at least once and in no particular order: drop a webhook-id you handled before,
    // order by webhook.Data.OccurredAt, and queue the work so you answer within 10 seconds.
    return Results.NoContent();
});
```

## ASP.NET Core Identity

`Postilio.Client.AspNetCore` implements Identity's `IEmailSender<TUser>`, so confirmation links and password resets go
out through Postilio, with Identity's own wording, as HTML and plain text:

```csharp
builder.Services.AddPostilio(builder.Configuration.GetSection("Postilio"));
builder.Services.AddPostilioEmailSender<ApplicationUser>(o => o.From = "Acme <no-reply@mail.example.com>");
```

The same registration serves the Identity UI's non-generic `Microsoft.AspNetCore.Identity.UI.Services.IEmailSender`
(scaffolded Identity pages), which sends the HTML it is given. Want your own wording? Implement either interface
yourself with a few lines around `PostilioClient.SendEmailAsync`.

## Replacing SmtpClient

Code that sends with `System.Net.Mail.SmtpClient` can keep its shape and swap the transport. Unlike SMTP, the API tells
you at once whether Postilio accepted the message and why not, and never sends a password over the wire unencrypted:

```csharp
// before: smtp.SendMailAsync(new MailMessage(from, to, subject, body) { IsBodyHtml = true });
await postilio.SendEmailAsync(new SendEmailRequest { From = from, To = [to], Subject = subject, Html = body }, ct);
```

SMTP stays available for software that only speaks SMTP: see the [sending guide](https://docs.postilio.eu/sending.html).

## Testing your code

Use a test key in your own tests and CI: nothing is delivered, and the recipient decides the outcome
(`delivered@`, `bounced@`, `deferred@`, `complained@`, `suppressed@simulator.postilio.eu`). Switching to live is
swapping the key.

## Versioning

[Semantic versioning](https://semver.org/). Until 1.0 a minor version may change the public API; every change is in
[CHANGELOG.md](CHANGELOG.md).

## Contributing and security

See [CONTRIBUTING.md](CONTRIBUTING.md) and [SECURITY.md](SECURITY.md).

## License

[MIT](LICENSE).
