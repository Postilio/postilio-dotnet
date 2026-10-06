namespace Postilio;

/// <summary>One event in a message's history.</summary>
public sealed class EmailEvent
{
    /// <summary>The event; see <see cref="EmailStatuses"/>.</summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>When it happened.</summary>
    public DateTimeOffset OccurredAt { get; init; }

    /// <summary>The receiving server's SMTP reply code, when there was one.</summary>
    public short? SmtpCode { get; init; }

    /// <summary>The receiving server's reply.</summary>
    public string? Response { get; init; }

    /// <summary>The delivery attempt.</summary>
    public short? Attempt { get; init; }

    /// <summary>The receiving server.</summary>
    public string? RemoteHost { get; init; }

    /// <summary>The enhanced status code, such as <c>5.1.1</c>.</summary>
    public string? EnhancedCode { get; init; }

    /// <summary>Why it bounced or was deferred, such as <c>InvalidRecipient</c>.</summary>
    public string? Classification { get; init; }
}
