using System.Net;

namespace Postilio;

/// <summary>403: the key lacks the scope of this call, or may not be used from this address.</summary>
public sealed class PostilioPermissionException : PostilioException
{
    /// <summary>Creates the exception for an error answer.</summary>
    public PostilioPermissionException(string message, HttpStatusCode statusCode, string? errorCode = null, string? traceId = null, TimeSpan? retryAfter = null)
        : base(message, statusCode, errorCode, traceId, retryAfter)
    {
    }
}
