using System.Net;

namespace Postilio;

/// <summary>400: a field is not valid; <see cref="Errors"/> lists the problems per field. Fix the request before sending it again.</summary>
public sealed class PostilioValidationException : PostilioException
{
    /// <summary>Creates the exception for a validation problem.</summary>
    public PostilioValidationException(string message, IReadOnlyDictionary<string, string[]> errors, string? traceId = null)
        : base(message, HttpStatusCode.BadRequest, traceId: traceId)
    {
        Errors = errors;
    }

    /// <summary>
    /// The problems per field, keyed by the request's field names; <c>body</c> stands for text and html together and
    /// <c>idempotencyKey</c> for the header. Empty when the body was not JSON or a field had the wrong type.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
