using System.Net;

namespace Postilio.Http;

/// <summary>
/// Retries a 429 for every call, since Postilio stored nothing. A connection failure or a 408/500/502/503/504 is retried
/// only when sending again cannot do anything twice: a GET, or a POST with an Idempotency-Key.
/// </summary>
internal sealed class RetryHandler(Func<PostilioOptions> options, Func<TimeSpan, CancellationToken, Task> delay) : DelegatingHandler
{
    internal const string IdempotencyKeyHeader = "Idempotency-Key";
    private static readonly TimeSpan FirstBackoff = TimeSpan.FromMilliseconds(500);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var settings = options();
        for (var attempt = 0; ; attempt++)
        {
            HttpResponseMessage response;
            try
            {
                response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }
            catch (HttpRequestException) when (attempt < settings.MaxRetries && IsReplayable(request))
            {
                await delay(Backoff(attempt), cancellationToken).ConfigureAwait(false);
                continue;
            }
            if (attempt >= settings.MaxRetries || WaitBeforeRetry(request, response, attempt) is not { } wait || wait > settings.MaxRetryDelay)
            {
                return response;
            }
            response.Dispose();
            await delay(wait, cancellationToken).ConfigureAwait(false);
        }
    }

    private static TimeSpan? WaitBeforeRetry(HttpRequestMessage request, HttpResponseMessage response, int attempt)
    {
        var retryable = response.StatusCode switch
        {
            HttpStatusCode.TooManyRequests => true,
            HttpStatusCode.RequestTimeout or HttpStatusCode.InternalServerError or HttpStatusCode.BadGateway
                or HttpStatusCode.ServiceUnavailable or HttpStatusCode.GatewayTimeout => IsReplayable(request),
            _ => false,
        };
        return retryable ? RetryAfter(response) ?? Backoff(attempt) : null;
    }

    internal static TimeSpan? RetryAfter(HttpResponseMessage response) => response.Headers.RetryAfter switch
    {
        { Delta: { } delta } => delta,
        { Date: { } date } => date - DateTimeOffset.UtcNow is var wait && wait > TimeSpan.Zero ? wait : TimeSpan.Zero,
        _ => null,
    };

    private static bool IsReplayable(HttpRequestMessage request) =>
        request.Method == HttpMethod.Get || (request.Method == HttpMethod.Post && request.Headers.Contains(IdempotencyKeyHeader));

    // 0.5 s, 1 s, 2 s, … with ±20% spread so clients that failed together do not retry together.
    private static TimeSpan Backoff(int attempt) =>
        FirstBackoff * Math.Pow(2, attempt) * (0.8 + (Random.Shared.NextDouble() * 0.4));
}
