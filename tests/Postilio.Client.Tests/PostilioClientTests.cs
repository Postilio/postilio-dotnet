using System.Net;
using System.Text.Json.Nodes;

namespace Postilio.Client.Tests;

public sealed class PostilioClientTests
{
    private const string ApiKey = "pk_test_abcdefghijklmnopqrstuvwxyz012345";
    private static readonly Guid EmailId = Guid.Parse("01a10ce5-a09a-788b-9243-d7c9c59773d2");
    private readonly StubHandler _http = new();

    [Fact]
    public async Task SendEmailAsync_Request_IsPostedAsTheApiExpects()
    {
        _http.Answer(HttpStatusCode.Accepted, $$"""{"ids":["{{EmailId}}"],"suppressed":[]}""");

        var result = await Client().SendEmailAsync(new SendEmailRequest
        {
            From = "Acme <no-reply@mail.example.com>",
            To = ["ada.lovelace@example.com"],
            Subject = "Your invoice",
            Text = "See the attachment.",
            Attachments = [new EmailAttachment { FileName = "invoice.pdf", ContentType = "application/pdf", Content = [1, 2, 3] }],
        }, cancellationToken: TestContext.Current.CancellationToken);

        var (request, body) = Assert.Single(_http.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("https://api.postilio.eu/v1/emails", request.RequestUri?.ToString());
        Assert.Equal($"Bearer {ApiKey}", request.Headers.Authorization?.ToString());
        Assert.Equal("application/json", request.Content?.Headers.ContentType?.MediaType);
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("""
            {
              "from": "Acme <no-reply@mail.example.com>",
              "to": ["ada.lovelace@example.com"],
              "subject": "Your invoice",
              "text": "See the attachment.",
              "attachments": [{ "fileName": "invoice.pdf", "contentType": "application/pdf", "content": "AQID" }]
            }
            """), JsonNode.Parse(body ?? string.Empty)), body);
        Assert.Equal([EmailId], result.Ids);
    }

    [Fact]
    public async Task SendEmailAsync_NoIdempotencyKey_SendsANewOneEveryCall()
    {
        _http.Answer(HttpStatusCode.Accepted, """{"ids":[],"suppressed":[]}""").Answer(HttpStatusCode.Accepted, """{"ids":[],"suppressed":[]}""");
        var client = Client();

        await client.SendEmailAsync(Welcome(), cancellationToken: TestContext.Current.CancellationToken);
        await client.SendEmailAsync(Welcome(), cancellationToken: TestContext.Current.CancellationToken);

        var keys = _http.Requests.Select(r => Assert.Single(r.Request.Headers.GetValues("Idempotency-Key"))).ToArray();
        Assert.All(keys, key => Assert.True(Guid.TryParse(key, out _), key));
        Assert.NotEqual(keys[0], keys[1]);
    }

    [Fact]
    public async Task SendEmailAsync_OwnIdempotencyKey_IsSent()
    {
        _http.Answer(HttpStatusCode.Accepted, """{"ids":[],"suppressed":[]}""");

        await Client().SendEmailAsync(Welcome(), "order-1042-receipt", TestContext.Current.CancellationToken);

        Assert.Equal("order-1042-receipt", Assert.Single(_http.Requests[0].Request.Headers.GetValues("Idempotency-Key")));
    }

    [Fact]
    public async Task GetEmailAsync_DocsExample_IsReadIntoTypedProperties()
    {
        _http.Answer(HttpStatusCode.OK, """
            {
              "id": "01a10ce5-a09a-788b-9243-d7c9c59773d2", "status": "bounced", "from": "no-reply@mail.example.com",
              "to": "bounced@simulator.postilio.eu", "subject": null, "tag": "welcome",
              "acceptedAt": "2026-10-05T16:28:57.882+00:00", "test": true, "via": "api",
              "events": [
                { "type": "bounced", "occurredAt": "2026-10-05T16:28:57.882+00:00", "smtpCode": 550, "response": "550 5.1.1 Simulated",
                  "attempt": 1, "remoteHost": "mx.simulator.postilio.eu", "enhancedCode": "5.1.1", "classification": "InvalidRecipient",
                  "someFieldAddedLater": 1 }
              ]
            }
            """);

        var email = await Client().GetEmailAsync(EmailId, TestContext.Current.CancellationToken);

        Assert.Equal("https://api.postilio.eu/v1/emails/01a10ce5-a09a-788b-9243-d7c9c59773d2", _http.Requests[0].Request.RequestUri?.ToString());
        Assert.Equal(EmailStatuses.Bounced, email.Status);
        Assert.Null(email.Subject);
        Assert.True(email.Test);
        Assert.Equal(new DateTimeOffset(2026, 10, 5, 16, 28, 57, 882, TimeSpan.Zero), email.AcceptedAt);
        var bounce = Assert.Single(email.Events);
        Assert.Equal((short)550, bounce.SmtpCode);
        Assert.Equal("InvalidRecipient", bounce.Classification);
    }

    [Fact]
    public async Task UpdateWebhookEndpointAsync_FieldsLeftNull_AreNotSent()
    {
        var id = Guid.NewGuid();
        _http.Answer(HttpStatusCode.OK, $$"""{"id":"{{id}}","url":"https://example.com/h","events":["delivered"],"mode":"live","paused":false,"secretHint":"…abcd","createdAt":"2026-10-05T16:28:57Z","lastDelivery":null}""");

        var endpoint = await Client().UpdateWebhookEndpointAsync(id, new UpdateWebhookEndpointRequest { Paused = false }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Patch, _http.Requests[0].Request.Method);
        Assert.Equal("""{"paused":false}""", _http.Requests[0].Body);
        Assert.Null(endpoint.LastDelivery);
    }

    [Fact]
    public async Task ListSuppressionsAsync_Filters_AreEscapedIntoTheQuery()
    {
        var before = Guid.NewGuid();
        _http.Answer(HttpStatusCode.OK, """{"data":[],"next":null}""");

        await Client().ListSuppressionsAsync("ada+test@example.com", SuppressionReasons.HardBounce, before, 20, TestContext.Current.CancellationToken);

        Assert.Equal($"https://api.postilio.eu/v1/suppressions?q=ada%2Btest%40example.com&reason=hard_bounce&before={before}&limit=20",
            _http.Requests[0].Request.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task DeleteSuppressionAsync_Reason_IsSentAsTheBody()
    {
        var id = Guid.NewGuid();
        _http.Answer(HttpStatusCode.NoContent);

        await Client().DeleteSuppressionAsync(id, new RemoveSuppressionRequest { Reason = "Subscribed again on 5 October." }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Delete, _http.Requests[0].Request.Method);
        Assert.Equal("""{"reason":"Subscribed again on 5 October."}""", _http.Requests[0].Body);
    }

    public static TheoryData<HttpStatusCode, string?, Type, string?> Errors() => new()
    {
        { HttpStatusCode.Unauthorized, """{"error":"invalid_api_key"}""", typeof(PostilioAuthenticationException), PostilioErrorCodes.InvalidApiKey },
        { HttpStatusCode.Forbidden, """{"error":"insufficient_scope"}""", typeof(PostilioPermissionException), PostilioErrorCodes.InsufficientScope },
        { HttpStatusCode.NotFound, null, typeof(PostilioNotFoundException), null },
        { HttpStatusCode.Conflict, """{"error":"domain_exists"}""", typeof(PostilioConflictException), PostilioErrorCodes.DomainExists },
        { HttpStatusCode.UnprocessableEntity, """{"error":"unverified_sender_domain"}""", typeof(PostilioUnprocessableException), PostilioErrorCodes.UnverifiedSenderDomain },
        { HttpStatusCode.TooManyRequests, """{"error":"sandbox_daily_limit_reached"}""", typeof(PostilioRateLimitException), PostilioErrorCodes.SandboxDailyLimitReached },
        { HttpStatusCode.ServiceUnavailable, """{"error":"dns_unavailable"}""", typeof(PostilioServerException), PostilioErrorCodes.DnsUnavailable },
        { HttpStatusCode.Conflict, """{"error":"a_code_added_later"}""", typeof(PostilioConflictException), "a_code_added_later" },
        { HttpStatusCode.Gone, "not json", typeof(PostilioException), null },
    };

    [Theory, MemberData(nameof(Errors))]
    public async Task Request_ErrorAnswer_ThrowsTheExceptionOfItsStatusWithItsCode(HttpStatusCode status, string? body, Type expected, string? code)
    {
        _http.Answer(status, body);

        var error = await Assert.ThrowsAnyAsync<PostilioException>(() => NoRetryClient().CreateDomainAsync(new CreateDomainRequest { Name = "mail.example.com" }, TestContext.Current.CancellationToken));

        Assert.IsType(expected, error);
        Assert.Equal(status, error.StatusCode);
        Assert.Equal(code, error.ErrorCode);
        Assert.Contains($"POST /v1/domains answered {(int)status}", error.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(ApiKey, error.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Request_RateLimited_CarriesRetryAfter()
    {
        _http.Answer(HttpStatusCode.TooManyRequests, """{"error":"sandbox_daily_limit_reached"}""", configure: r => r.Headers.TryAddWithoutValidation("Retry-After", "3600"));

        var error = await Assert.ThrowsAsync<PostilioRateLimitException>(() => Client().SendEmailAsync(Welcome(), cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(TimeSpan.FromHours(1), error.RetryAfter);
    }

    [Fact]
    public async Task Request_ValidationProblem_ListsTheProblemsPerField()
    {
        _http.Answer(HttpStatusCode.BadRequest, """
            {"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,
             "errors":{"to":["Between 1 and 50 recipients are required."],"body":["Either text or html is required."]},
             "traceId":"00-4d6a1c9b71484cc59ac852cda603b93c-28de36d1c118bc08-00"}
            """, "application/problem+json");

        var error = await Assert.ThrowsAsync<PostilioValidationException>(() => Client().SendEmailAsync(Welcome(), cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(["Between 1 and 50 recipients are required."], error.Errors["to"]);
        Assert.Equal(["Either text or html is required."], error.Errors["body"]);
        Assert.Equal("00-4d6a1c9b71484cc59ac852cda603b93c-28de36d1c118bc08-00", error.TraceId);
    }

    [Fact]
    public async Task Request_BadRequestWithoutBody_IsAValidationProblemWithoutFields()
    {
        _http.Answer(HttpStatusCode.BadRequest);

        var error = await Assert.ThrowsAsync<PostilioValidationException>(() => Client().SendEmailAsync(Welcome(), cancellationToken: TestContext.Current.CancellationToken));

        Assert.Empty(error.Errors);
    }

    [Fact]
    public async Task Request_ServerError_CarriesTheTraceId()
    {
        _http.Answer(HttpStatusCode.InternalServerError, """{"title":"An error occurred.","status":500,"traceId":"00-abc-def-00"}""", "application/problem+json");

        var error = await Assert.ThrowsAsync<PostilioServerException>(() => NoRetryClient().GetEmailAsync(EmailId, TestContext.Current.CancellationToken));

        Assert.Equal("00-abc-def-00", error.TraceId);
    }

    [Fact]
    public void Options_ToString_DoesNotShowTheApiKey()
    {
        Assert.DoesNotContain(ApiKey, new PostilioOptions { ApiKey = ApiKey }.ToString(), StringComparison.Ordinal);
    }

    private static SendEmailRequest Welcome() => new()
    {
        From = "Acme <no-reply@mail.example.com>",
        To = ["delivered@simulator.postilio.eu"],
        Subject = "Welcome",
        Text = "Hi.",
    };

    private PostilioClient Client() => new(new PostilioOptions { ApiKey = ApiKey }, _http, (_, _) => Task.CompletedTask);

    private PostilioClient NoRetryClient() => new(new PostilioOptions { ApiKey = ApiKey, MaxRetries = 0 }, _http, (_, _) => Task.CompletedTask);
}
