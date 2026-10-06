using System.Net;

namespace Postilio;

/// <summary>404: no such resource in this key's project and mode.</summary>
public sealed class PostilioNotFoundException : PostilioException
{
    /// <summary>Creates the exception for an error answer.</summary>
    public PostilioNotFoundException(string message, HttpStatusCode statusCode, string? errorCode = null, string? traceId = null, TimeSpan? retryAfter = null)
        : base(message, statusCode, errorCode, traceId, retryAfter)
    {
    }
}
