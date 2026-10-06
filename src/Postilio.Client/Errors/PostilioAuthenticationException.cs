using System.Net;

namespace Postilio;

/// <summary>401: the API key is missing, unknown or revoked (<c>invalid_api_key</c>).</summary>
public sealed class PostilioAuthenticationException : PostilioException
{
    /// <summary>Creates the exception for an error answer.</summary>
    public PostilioAuthenticationException(string message, HttpStatusCode statusCode, string? errorCode = null, string? traceId = null, TimeSpan? retryAfter = null)
        : base(message, statusCode, errorCode, traceId, retryAfter)
    {
    }
}
