using System.Net;

namespace Postilio;

/// <summary>409: the request conflicts with what is there, such as <c>domain_exists</c>.</summary>
public sealed class PostilioConflictException : PostilioException
{
    /// <summary>Creates the exception for an error answer.</summary>
    public PostilioConflictException(string message, HttpStatusCode statusCode, string? errorCode = null, string? traceId = null, TimeSpan? retryAfter = null)
        : base(message, statusCode, errorCode, traceId, retryAfter)
    {
    }
}
