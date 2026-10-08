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
            Cc = ["account-manager@example.com"],
            Bcc = ["archive@example.com"],
            Subject = "Your invoice",
            Text = "See the attachment.",
            Attachments = [new EmailAttachment { FileName = "invoice.pdf", ContentType = "application/pdf", Content = [1, 2, 3] }],
            Headers = new Dictionary<string, string> { ["List-Unsubscribe"] = "<https://example.com/u/3f9a>", ["List-Unsubscribe-Post"] = "List-Unsubscribe=One-Click" },
            SendAt = new DateTimeOffset(2026, 11, 2, 9, 0, 0, TimeSpan.FromHours(1)),
        }, TestContext.Current.CancellationToken);

        var (request, body) = Assert.Single(_http.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("https://api.postilio.eu/v1/emails", request.RequestUri?.ToString());
        Assert.Equal($"Bearer {ApiKey}", request.Headers.Authorization?.ToString());
        Assert.StartsWith("postilio-dotnet/", request.Headers.UserAgent.ToString(), StringComparison.Ordinal);
        Assert.Equal("application/json", request.Content?.Headers.ContentType?.MediaType);
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("""
            {
              "from": "Acme <no-reply@mail.example.com>",
              "to": ["ada.lovelace@example.com"],
              "cc": ["account-manager@example.com"],
              "bcc": ["archive@example.com"],
              "subject": "Your invoice",
              "text": "See the attachment.",
              "attachments": [{ "fileName": "invoice.pdf", "contentType": "application/pdf", "content": "AQID" }],
              "headers": { "List-Unsubscribe": "<https://example.com/u/3f9a>", "List-Unsubscribe-Post": "List-Unsubscribe=One-Click" },
              "sendAt": "2026-11-02T09:00:00+01:00"
            }
            """), JsonNode.Parse(body ?? string.Empty)), body);
        Assert.Equal([EmailId], result.Ids);
    }

    [Fact]
    public async Task SendEmailAsync_NoIdempotencyKey_SendsANewOneEveryCall()
    {
        _http.Answer(HttpStatusCode.Accepted, """{"ids":[],"suppressed":[]}""").Answer(HttpStatusCode.Accepted, """{"ids":[],"suppressed":[]}""");
        var client = Client();

        await client.SendEmailAsync(Welcome(), TestContext.Current.CancellationToken);
        await client.SendEmailAsync(Welcome(), TestContext.Current.CancellationToken);

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
              "acceptedAt": "2026-10-05T16:28:57.882+00:00", "test": true, "via": "api", "sendAt": "2026-10-05T17:00:00+00:00",
              "events": [
                { "type": "bounced", "occurredAt": "2026-10-05T16:28:57.882+00:00", "smtpCode": 550, "response": "550 5.1.1 Simulated",
                  "reason": "recipient_rejected",
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
        Assert.Equal(new DateTimeOffset(2026, 10, 5, 17, 0, 0, TimeSpan.Zero), email.SendAt);
        var bounce = Assert.Single(email.Events);
        Assert.Equal((short)550, bounce.SmtpCode);
        Assert.Equal("InvalidRecipient", bounce.Classification);
        Assert.Equal(EmailEventReasons.RecipientRejected, bounce.Reason);
    }

    [Fact]
    public async Task CancelEmailAsync_Id_IsDeletedWithoutABody()
    {
        _http.Answer(HttpStatusCode.NoContent);

        await Client().CancelEmailAsync(EmailId, TestContext.Current.CancellationToken);

        var (request, body) = Assert.Single(_http.Requests);
        Assert.Equal(HttpMethod.Delete, request.Method);
        Assert.Equal("https://api.postilio.eu/v1/emails/01a10ce5-a09a-788b-9243-d7c9c59773d2", request.RequestUri?.ToString());
        Assert.Null(body);
    }

    [Fact]
    public async Task SendTestEmailAsync_Request_IsPostedToTheTestEndpoint()
    {
        _http.Answer(HttpStatusCode.Accepted, $$"""{"id":"{{EmailId}}"}""");

        var sent = await Client().SendTestEmailAsync(new TestEmailRequest { From = "no-reply@mail.example.com", To = "ada@example.org" }, TestContext.Current.CancellationToken);

        var (request, body) = Assert.Single(_http.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("https://api.postilio.eu/v1/emails/test", request.RequestUri?.ToString());
        Assert.Equal("""{"from":"no-reply@mail.example.com","to":"ada@example.org"}""", body);
        Assert.Equal(EmailId, sent.Id);
    }

    [Fact]
    public async Task GetUsageAsync_DocsExample_IsReadIntoTypedProperties()
    {
        _http.Answer(HttpStatusCode.OK, """
            {
              "month": "2026-10", "final": false, "resetsAt": "2026-11-01T00:00:00+00:00",
              "organization": { "plan": "growth", "state": "warning" },
              "project": { "id": "0199b3c0-6f3a-7d2e-9a41-2c8e5d7f1a20", "billable": 12040, "accepted": 12101, "suppressed": 61,
                           "delivered": 11950, "bounced": 88, "complained": 2 },
              "apiKey": { "id": "0199b3c4-2b71-7c05-8e6d-5a9f3c1e7b42", "accepted": 8020, "sent": 7981, "suppressed": 39,
                          "delivered": 7915, "bounced": 64, "complained": 2 }
            }
            """);

        var usage = await Client().GetUsageAsync("2026-10", TestContext.Current.CancellationToken);

        Assert.Equal("https://api.postilio.eu/v1/usage?month=2026-10", _http.Requests[0].Request.RequestUri?.ToString());
        Assert.Equal("warning", usage.Organization.State);
        Assert.Equal(12040, usage.Project.Billable);
        Assert.Equal(7981, usage.ApiKey.Sent);
        Assert.Equal(new DateTimeOffset(2026, 11, 1, 0, 0, 0, TimeSpan.Zero), usage.ResetsAt);
    }

    [Fact]
    public async Task GetDomainAsync_Dmarc_IsRead()
    {
        var id = Guid.NewGuid();
        _http.Answer(HttpStatusCode.OK, $$$"""
            {"id":"{{{id}}}","name":"mail.example.com","status":"verified","checkedAt":"2026-10-07T10:00:00Z","records":[],
             "createdAt":"2026-10-01T10:00:00Z","addedBy":null,"failingSince":null,"sent30d":0,
             "dmarc":{"status":"monitoring","policyDomain":"mail.example.com","records":["v=DMARC1; p=none"],"issues":["no_reports"]}}
            """);

        var domain = await Client().GetDomainAsync(id, TestContext.Current.CancellationToken);

        Assert.Equal("monitoring", domain.Dmarc?.Status);
        Assert.Equal("mail.example.com", domain.Dmarc?.PolicyDomain);
        Assert.Equal(["v=DMARC1; p=none"], domain.Dmarc?.Records);
        Assert.Equal(["no_reports"], domain.Dmarc?.Issues);
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
        { HttpStatusCode.RequestEntityTooLarge, """{"error":"message_too_large"}""", typeof(PostilioException), PostilioErrorCodes.MessageTooLarge },
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
        Assert.StartsWith($"POST /v1/domains answered {(int)status}", error.Message, StringComparison.Ordinal);
        Assert.EndsWith(".", error.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(ApiKey, error.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Request_ErrorWithMessage_ShowsTheMessage()
    {
        _http.Answer(HttpStatusCode.RequestEntityTooLarge, """{"error":"message_too_large","message":"A message may be at most 10485760 bytes; this one is 10485761."}""");

        var error = await Assert.ThrowsAnyAsync<PostilioException>(() => NoRetryClient().SendEmailAsync(Welcome(), TestContext.Current.CancellationToken));

        Assert.Equal("POST /v1/emails answered 413 (message_too_large): A message may be at most 10485760 bytes; this one is 10485761.", error.Message);
    }

    [Fact]
    public async Task Request_RateLimited_CarriesRetryAfter()
    {
        _http.Answer(HttpStatusCode.TooManyRequests, """{"error":"sandbox_daily_limit_reached"}""", configure: r => r.Headers.TryAddWithoutValidation("Retry-After", "3600"));

        var error = await Assert.ThrowsAsync<PostilioRateLimitException>(() => Client().SendEmailAsync(Welcome(), TestContext.Current.CancellationToken));

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

        var error = await Assert.ThrowsAsync<PostilioValidationException>(() => Client().SendEmailAsync(Welcome(), TestContext.Current.CancellationToken));

        Assert.Equal(["Between 1 and 50 recipients are required."], error.Errors["to"]);
        Assert.Equal(["Either text or html is required."], error.Errors["body"]);
        Assert.Equal("00-4d6a1c9b71484cc59ac852cda603b93c-28de36d1c118bc08-00", error.TraceId);
    }

    [Fact]
    public async Task Request_BadRequestWithoutBody_IsAValidationProblemWithoutFields()
    {
        _http.Answer(HttpStatusCode.BadRequest);

        var error = await Assert.ThrowsAsync<PostilioValidationException>(() => Client().SendEmailAsync(Welcome(), TestContext.Current.CancellationToken));

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
    public async Task Constructor_HttpClientOfTheCaller_IsLeftAsItWas()
    {
        using var shared = new HttpClient(_http, disposeHandler: false) { BaseAddress = new Uri("https://other.example.com/") };
        _http.Answer(HttpStatusCode.OK, """{"data":[]}""");

        await new PostilioClient(shared, new PostilioOptions { ApiKey = ApiKey }).ListDomainsAsync(TestContext.Current.CancellationToken);

        Assert.Equal(new Uri("https://other.example.com/"), shared.BaseAddress);
        Assert.Null(shared.DefaultRequestHeaders.Authorization);
        Assert.Equal("https://api.postilio.eu/v1/domains", _http.Requests[0].Request.RequestUri?.ToString());
        Assert.Equal($"Bearer {ApiKey}", _http.Requests[0].Request.Headers.Authorization?.ToString());
    }

    [Fact]
    public async Task Request_SuccessWithoutJson_ThrowsAPostilioException()
    {
        _http.Answer(HttpStatusCode.OK, "<html>proxy</html>", "text/html");

        var error = await Assert.ThrowsAsync<PostilioException>(() => Client().GetEmailAsync(EmailId, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.OK, error.StatusCode);
        Assert.IsType<System.Text.Json.JsonException>(error.InnerException);
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
