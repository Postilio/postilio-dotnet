namespace Postilio;

/// <summary>The answer to a send: the accepted messages.</summary>
public sealed class SendEmailResponse
{
    /// <summary>One message id per recipient, in the order of <see cref="SendEmailRequest.To"/>, then <see cref="SendEmailRequest.Cc"/>, then <see cref="SendEmailRequest.Bcc"/>.</summary>
    public IReadOnlyList<Guid> Ids { get; init; } = [];

    /// <summary>Recipients on the suppression list: accepted, but not sent.</summary>
    public IReadOnlyList<string> Suppressed { get; init; } = [];
}
