using System.Net;

namespace Postilio;

/// <summary>429: a limit was reached; wait <see cref="PostilioException.RetryAfter"/>.</summary>
public sealed class PostilioRateLimitException : PostilioException
{
    /// <summary>Creates the exception for an error answer.</summary>
    public PostilioRateLimitException(string message, HttpStatusCode statusCode, string? errorCode = null, string? traceId = null, TimeSpan? retryAfter = null)
        : base(message, statusCode, errorCode, traceId, retryAfter)
    {
    }
}
