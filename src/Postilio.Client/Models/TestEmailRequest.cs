namespace Postilio;

/// <summary>
/// A test email to one address of your own. Without <see cref="Subject"/>, <see cref="Text"/> and <see cref="Html"/>
/// Postilio sends its sample message.
/// </summary>
public sealed class TestEmailRequest
{
    /// <summary>An address on a verified domain of the project.</summary>
    public required string From { get; init; }

    /// <summary>One address: confirmed for test emails in the project, or a member's of its organization.</summary>
    public required string To { get; init; }

    /// <summary>With <see cref="Text"/> or <see cref="Html"/>, your own message instead of the sample.</summary>
    public string? Subject { get; init; }

    /// <summary>The plain-text body of your own message.</summary>
    public string? Text { get; init; }

    /// <summary>The HTML body of your own message.</summary>
    public string? Html { get; init; }
}
