using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Postilio.Client.Tests;

public sealed class ServiceCollectionTests
{
    private const string ApiKey = "pk_test_abcdefghijklmnopqrstuvwxyz012345";
    private const string Domain = """{"id":"01a1081b-eb5c-7685-ab38-fdd4a4ab10e5","name":"mail.example.com","status":"pending","records":[],"createdAt":"2026-10-04T18:10:09Z","sent30d":0}""";
    private readonly StubHandler _http = new();
    private readonly LogCollector _logs = new();

    [Fact]
    public async Task AddPostilio_FromConfiguration_SendsWithThatKeyAndAddressAndRetries()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Postilio:ApiKey"] = ApiKey,
            ["Postilio:BaseAddress"] = "http://localhost:26299",
            ["Postilio:MaxRetries"] = "1",
            ["Postilio:MaxRetryDelay"] = "00:00:05",
        }).Build();
        var services = new ServiceCollection();
        services.AddPostilio(configuration.GetSection("Postilio")).ConfigurePrimaryHttpMessageHandler(() => _http);
        _http.Answer(HttpStatusCode.TooManyRequests, """{"error":"too_many_checks"}""", configure: r => r.Headers.TryAddWithoutValidation("Retry-After", "0"))
            .Answer(HttpStatusCode.OK, Domain);
        using var provider = services.BuildServiceProvider();

        await provider.GetRequiredService<PostilioClient>().CheckDomainAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        Assert.Equal(2, _http.Requests.Count);
        Assert.StartsWith("http://localhost:26299/v1/domains/", _http.Requests[1].Request.RequestUri?.ToString(), StringComparison.Ordinal);
        Assert.Equal($"Bearer {ApiKey}", _http.Requests[1].Request.Headers.Authorization?.ToString());
        var options = provider.GetRequiredService<IOptions<PostilioOptions>>().Value;
        Assert.Equal((1, TimeSpan.FromSeconds(5)), (options.MaxRetries, options.MaxRetryDelay));
    }

    [Fact]
    public async Task AddPostilio_TraceLogging_NeverWritesTheApiKey()
    {
        var services = new ServiceCollection();
        services.AddLogging(b => b.SetMinimumLevel(LogLevel.Trace).AddProvider(_logs));
        services.AddPostilio(o => o.ApiKey = ApiKey).ConfigurePrimaryHttpMessageHandler(() => _http);
        _http.Answer(HttpStatusCode.Unauthorized, """{"error":"invalid_api_key"}""");
        using var provider = services.BuildServiceProvider();

        await Assert.ThrowsAsync<PostilioAuthenticationException>(() => provider.GetRequiredService<PostilioClient>().ListDomainsAsync(TestContext.Current.CancellationToken));

        Assert.Contains(_logs.Lines, line => line.Contains("Authorization", StringComparison.Ordinal));
        Assert.DoesNotContain(_logs.Lines, line => line.Contains(ApiKey, StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("")]
    [InlineData("whsec_MfKQ9r8GKYqrTwjUPD8ILPZIo2LaLaSw")]
    public void AddPostilio_NoApiKey_FailsWhenTheClientIsResolved(string apiKey)
    {
        var services = new ServiceCollection();
        services.AddPostilio(o => o.ApiKey = apiKey);
        using var provider = services.BuildServiceProvider();

        var error = Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<PostilioClient>());

        Assert.Contains("ApiKey", error.Message, StringComparison.Ordinal);
    }

    private sealed class LogCollector : ILoggerProvider, ILogger
    {
        public List<string> Lines { get; } = [];

        public ILogger CreateLogger(string categoryName) => this;

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            lock (Lines)
            {
                Lines.Add(formatter(state, exception));
            }
        }

        public void Dispose()
        {
        }
    }
}
