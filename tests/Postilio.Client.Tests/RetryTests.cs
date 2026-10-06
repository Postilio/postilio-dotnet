using System.Net;

namespace Postilio.Client.Tests;

public sealed class RetryTests
{
    private const string ApiKey = "pk_test_abcdefghijklmnopqrstuvwxyz012345";
    private const string Domain = """{"id":"01a1081b-eb5c-7685-ab38-fdd4a4ab10e5","name":"mail.example.com","status":"pending","records":[],"createdAt":"2026-10-04T18:10:09Z","sent30d":0}""";
    private const string Sent = """{"ids":[],"suppressed":[]}""";
    private readonly StubHandler _http = new();
    private readonly List<TimeSpan> _delays = [];

    [Fact]
    public async Task RateLimited_RetryAfterWithinTheMaximum_WaitsThatLongAndRetriesAnyMethod()
    {
        _http.Answer(HttpStatusCode.TooManyRequests, """{"error":"too_many_checks"}""", configure: RetryAfter(30)).Answer(HttpStatusCode.OK, Domain);

        await Client().CheckDomainAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        Assert.Equal(2, _http.Requests.Count);
        Assert.Equal([TimeSpan.FromSeconds(30)], _delays);
    }

    [Fact]
    public async Task RateLimited_RetryAfterBeyondTheMaximum_ThrowsAtOnce()
    {
        _http.Answer(HttpStatusCode.TooManyRequests, """{"error":"too_many_checks"}""", configure: RetryAfter(31));

        var error = await Assert.ThrowsAsync<PostilioRateLimitException>(() => Client().CheckDomainAsync(Guid.NewGuid(), TestContext.Current.CancellationToken));

        Assert.Single(_http.Requests);
        Assert.Empty(_delays);
        Assert.Equal(TimeSpan.FromSeconds(31), error.RetryAfter);
    }

    [Fact]
    public async Task RateLimited_RetryAfterAsADate_WaitsUntilThen()
    {
        _http.Answer(HttpStatusCode.TooManyRequests, """{"error":"sandbox_rate_limit_reached"}""",
            configure: r => r.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(DateTimeOffset.UtcNow.AddSeconds(10)))
            .Answer(HttpStatusCode.Accepted, Sent);

        await Client().SendEmailAsync(Welcome(), TestContext.Current.CancellationToken);

        var delay = Assert.Single(_delays);
        Assert.InRange(delay, TimeSpan.FromSeconds(8), TimeSpan.FromSeconds(10));
    }

    public static TheoryData<HttpStatusCode> Transient() => [HttpStatusCode.RequestTimeout, HttpStatusCode.InternalServerError, HttpStatusCode.BadGateway, HttpStatusCode.ServiceUnavailable, HttpStatusCode.GatewayTimeout];

    [Theory, MemberData(nameof(Transient))]
    public async Task TransientError_Get_IsRetriedWithGrowingDelays(HttpStatusCode status)
    {
        _http.Answer(status).Answer(status).Answer(HttpStatusCode.OK, Domain);

        await Client().GetDomainAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        Assert.Equal(3, _http.Requests.Count);
        Assert.Equal(2, _delays.Count);
        Assert.InRange(_delays[0], TimeSpan.FromMilliseconds(400), TimeSpan.FromMilliseconds(600));
        Assert.InRange(_delays[1], TimeSpan.FromMilliseconds(800), TimeSpan.FromMilliseconds(1200));
    }

    [Fact]
    public async Task TransientError_ManyRetries_NeverWaitsLongerThanTheMaximum()
    {
        _http.Fail().Fail().Fail().Fail().Fail().Answer(HttpStatusCode.OK, Domain);
        var client = new PostilioClient(new PostilioOptions { ApiKey = ApiKey, MaxRetries = 5, MaxRetryDelay = TimeSpan.FromSeconds(3) }, _http, (delay, _) =>
        {
            _delays.Add(delay);
            return Task.CompletedTask;
        });

        await client.GetDomainAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        Assert.Equal(TimeSpan.FromSeconds(3), _delays.Max());
    }

    [Fact]
    public async Task TransientError_SendEmail_IsRetriedWithTheSameIdempotencyKey()
    {
        _http.Fail().Answer(HttpStatusCode.BadGateway).Answer(HttpStatusCode.Accepted, Sent);

        await Client().SendEmailAsync(Welcome(), TestContext.Current.CancellationToken);

        Assert.Equal(3, _http.Requests.Count);
        Assert.Single(_http.Requests.SelectMany(r => r.Request.Headers.GetValues("Idempotency-Key")).Distinct());
        Assert.All(_http.Requests, r => Assert.Equal(_http.Requests[0].Body, r.Body));
    }

    [Fact]
    public async Task TransientError_PostWithoutIdempotencyKey_IsNotRetried()
    {
        _http.Answer(HttpStatusCode.BadGateway);

        await Assert.ThrowsAsync<PostilioServerException>(() => Client().CreateDomainAsync(new CreateDomainRequest { Name = "mail.example.com" }, TestContext.Current.CancellationToken));

        Assert.Single(_http.Requests);
    }

    [Fact]
    public async Task TransientError_Delete_IsNotRetried()
    {
        _http.Answer(HttpStatusCode.ServiceUnavailable);

        await Assert.ThrowsAsync<PostilioServerException>(() => Client().DeleteDomainAsync(Guid.NewGuid(), TestContext.Current.CancellationToken));

        Assert.Single(_http.Requests);
    }

    [Fact]
    public async Task ConnectionFailure_PostWithoutIdempotencyKey_IsThrown()
    {
        _http.Fail();

        await Assert.ThrowsAsync<HttpRequestException>(() => Client().RotateWebhookSecretAsync(Guid.NewGuid(), TestContext.Current.CancellationToken));

        Assert.Single(_http.Requests);
    }

    [Fact]
    public async Task ConnectionFailure_Get_IsRetriedUpToMaxRetries()
    {
        _http.Fail().Fail().Fail().Answer(HttpStatusCode.OK, Domain);

        await Assert.ThrowsAsync<HttpRequestException>(() => Client().GetDomainAsync(Guid.NewGuid(), TestContext.Current.CancellationToken));

        Assert.Equal(3, _http.Requests.Count);
    }

    [Fact]
    public async Task TransientError_EveryAttempt_GivesUpAfterMaxRetries()
    {
        _http.Answer(HttpStatusCode.ServiceUnavailable).Answer(HttpStatusCode.ServiceUnavailable).Answer(HttpStatusCode.ServiceUnavailable).Answer(HttpStatusCode.OK, Domain);

        await Assert.ThrowsAsync<PostilioServerException>(() => Client().GetDomainAsync(Guid.NewGuid(), TestContext.Current.CancellationToken));

        Assert.Equal(3, _http.Requests.Count);
    }

    [Fact]
    public async Task TransientError_ServerAsksToWaitBeyondTheMaximum_IsNotRetried()
    {
        _http.Answer(HttpStatusCode.ServiceUnavailable, configure: RetryAfter(120));

        await Assert.ThrowsAsync<PostilioServerException>(() => Client().GetDomainAsync(Guid.NewGuid(), TestContext.Current.CancellationToken));

        Assert.Single(_http.Requests);
    }

    [Fact]
    public async Task ClientError_Get_IsNotRetried()
    {
        _http.Answer(HttpStatusCode.NotFound);

        await Assert.ThrowsAsync<PostilioNotFoundException>(() => Client().GetDomainAsync(Guid.NewGuid(), TestContext.Current.CancellationToken));

        Assert.Single(_http.Requests);
    }

    [Fact]
    public async Task Cancelled_WhileWaiting_StopsRetrying()
    {
        using var cancellation = new CancellationTokenSource();
        _http.Answer(HttpStatusCode.ServiceUnavailable).Answer(HttpStatusCode.OK, Domain);
        var client = new PostilioClient(new PostilioOptions { ApiKey = ApiKey }, _http, (_, ct) =>
        {
            cancellation.Cancel();
            return Task.Delay(Timeout.Infinite, ct);
        });

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.GetDomainAsync(Guid.NewGuid(), cancellation.Token));

        Assert.Single(_http.Requests);
    }

    private static Action<HttpResponseMessage> RetryAfter(int seconds) => r => r.Headers.TryAddWithoutValidation("Retry-After", seconds.ToString(System.Globalization.CultureInfo.InvariantCulture));

    private static SendEmailRequest Welcome() => new()
    {
        From = "Acme <no-reply@mail.example.com>",
        To = ["delivered@simulator.postilio.eu"],
        Subject = "Welcome",
        Text = "Hi.",
    };

    private PostilioClient Client() => new(new PostilioOptions { ApiKey = ApiKey }, _http, (delay, _) =>
    {
        _delays.Add(delay);
        return Task.CompletedTask;
    });
}
