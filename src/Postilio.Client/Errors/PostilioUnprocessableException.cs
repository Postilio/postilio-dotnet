using System.Net;

namespace Postilio;

/// <summary>422: valid, but it cannot be done now, such as <c>unverified_sender_domain</c>.</summary>
public sealed class PostilioUnprocessableException : PostilioException
{
    /// <summary>Creates the exception for an error answer.</summary>
    public PostilioUnprocessableException(string message, HttpStatusCode statusCode, string? errorCode = null, string? traceId = null, TimeSpan? retryAfter = null)
        : base(message, statusCode, errorCode, traceId, retryAfter)
    {
    }
}
