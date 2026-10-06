using System.Net;

namespace Postilio;

/// <summary>
/// Postilio answered with an error. Catch the subclass of a status, or check <see cref="ErrorCode"/> against
/// <see cref="PostilioErrorCodes"/>; handle a code you do not know by its status.
/// </summary>
public class PostilioException : Exception
{
    /// <summary>Creates the exception for an error answer.</summary>
    public PostilioException(string message, HttpStatusCode statusCode, string? errorCode = null, string? traceId = null, TimeSpan? retryAfter = null)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        TraceId = traceId;
        RetryAfter = retryAfter;
    }

    /// <summary>The HTTP status code of the answer.</summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>The stable error code, such as <c>domain_exists</c>; null when the answer had none (a 404, say).</summary>
    public string? ErrorCode { get; }

    /// <summary>The trace id of a server error: quote it when you contact support.</summary>
    public string? TraceId { get; }

    /// <summary>How long to wait before trying again, when Postilio said so.</summary>
    public TimeSpan? RetryAfter { get; }
}
