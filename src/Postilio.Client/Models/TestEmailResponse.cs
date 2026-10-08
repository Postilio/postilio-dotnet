namespace Postilio;

/// <summary>The answer to a test email: the message, to follow with <see cref="PostilioClient.GetEmailAsync"/>.</summary>
public sealed class TestEmailResponse
{
    /// <summary>The message id.</summary>
    public Guid Id { get; init; }
}
