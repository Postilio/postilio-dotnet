namespace Postilio;

/// <summary>An email to send: one message per recipient in <see cref="To"/>.</summary>
public sealed class SendEmailRequest
{
    /// <summary>An address on a verified domain of the project, optionally with a name: <c>Acme &lt;no-reply@mail.example.com&gt;</c>.</summary>
    public required string From { get; init; }

    /// <summary>1 to 50 bare addresses, without names.</summary>
    public required IReadOnlyList<string> To { get; init; }

    /// <summary>At most 998 characters.</summary>
    public required string Subject { get; init; }

    /// <summary>The plain-text body. At least one of <see cref="Text"/> and <see cref="Html"/> is required.</summary>
    public string? Text { get; init; }

    /// <summary>The HTML body. At least one of <see cref="Text"/> and <see cref="Html"/> is required.</summary>
    public string? Html { get; init; }

    /// <summary>Up to 64 letters, digits, <c>-</c> or <c>_</c>; shown in the portal and in webhook events.</summary>
    public string? Tag { get; init; }

    /// <summary>One address, with or without a name.</summary>
    public string? ReplyTo { get; init; }

    /// <summary>Up to 20 files of 10 MB together.</summary>
    public IReadOnlyList<EmailAttachment>? Attachments { get; init; }
}
