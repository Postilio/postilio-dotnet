namespace Postilio;

/// <summary>An email to send: one message per recipient in <see cref="To"/>, <see cref="Cc"/> and <see cref="Bcc"/>.</summary>
public sealed class SendEmailRequest
{
    /// <summary>An address on a verified domain of the project, optionally with a name: <c>Acme &lt;no-reply@mail.example.com&gt;</c>.</summary>
    public required string From { get; init; }

    /// <summary>1 to 50 bare addresses, without names.</summary>
    public required IReadOnlyList<string> To { get; init; }

    /// <summary>
    /// Bare addresses every copy shows in its Cc header; each gets a message of its own. Only with exactly one address
    /// in <see cref="To"/> (422 <c>cc_bcc_require_single_to</c>).
    /// </summary>
    public IReadOnlyList<string>? Cc { get; init; }

    /// <summary>
    /// Bare addresses that get a copy but appear in no header; each gets a message of its own. Only with exactly one
    /// address in <see cref="To"/>. <see cref="To"/>, <see cref="Cc"/> and <see cref="Bcc"/> hold at most 50 addresses
    /// together, each once.
    /// </summary>
    public IReadOnlyList<string>? Bcc { get; init; }

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

    /// <summary>
    /// Up to 20 files. The message, attachments included, is at most 10 MB, and its size times its recipients at most
    /// 25 MB (413).
    /// </summary>
    public IReadOnlyList<EmailAttachment>? Attachments { get; init; }

    /// <summary>
    /// Up to 10 headers of your own: <c>List-Unsubscribe</c>, <c>List-Unsubscribe-Post</c>, <c>In-Reply-To</c>,
    /// <c>References</c> and <c>X-</c> headers, with printable ASCII values of at most 900 characters. Any other, or one
    /// that breaks a rule, answers 400 under <c>headers</c>; see the sending guide.
    /// </summary>
    public IReadOnlyDictionary<string, string>? Headers { get; init; }

    /// <summary>
    /// When to send it: at least a minute and at most 30 days ahead (422 <c>send_at_too_soon</c>,
    /// <c>send_at_too_far</c>). Until then it is <c>scheduled</c> and <see cref="PostilioClient.CancelEmailAsync"/>
    /// cancels it. Null sends it at once. A test key checks it but simulates the message at once, so nothing waits.
    /// </summary>
    public DateTimeOffset? SendAt { get; init; }
}
