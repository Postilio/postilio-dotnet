using System.Net;

namespace Postilio;

/// <summary>5xx: an error at Postilio, or DNS gave no answer to a domain check (<c>dns_unavailable</c>).</summary>
public sealed class PostilioServerException : PostilioException
{
    /// <summary>Creates the exception for an error answer.</summary>
    public PostilioServerException(string message, HttpStatusCode statusCode, string? errorCode = null, string? traceId = null, TimeSpan? retryAfter = null)
        : base(message, statusCode, errorCode, traceId, retryAfter)
    {
    }
}
