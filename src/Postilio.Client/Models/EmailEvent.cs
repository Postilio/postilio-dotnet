namespace Postilio;

/// <summary>One event in a message's history.</summary>
public sealed class EmailEvent
{
    /// <summary>The event; see <see cref="EmailStatuses"/>.</summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>When it happened.</summary>
    public DateTimeOffset OccurredAt { get; init; }

    /// <summary>The receiving server's SMTP reply code; null when no server replied.</summary>
    public short? SmtpCode { get; init; }

    /// <summary>The receiving server's reply, with addresses masked; when no server replied, a sentence that explains what happened.</summary>
    public string? Response { get; init; }

    /// <summary>
    /// Why the attempt was delayed or failed, or why a scheduled message was canceled: see
    /// <see cref="EmailEventReasons"/>, or for a message canceled at its due time the code in <see cref="PostilioErrorCodes"/>
    /// that would have refused it then, such as <c>unverified_sender_domain</c> or <c>plan_daily_limit_reached</c>.
    /// Null when there is nothing to explain. Build on this, not on <see cref="Response"/>.
    /// </summary>
    public string? Reason { get; init; }

    /// <summary>True for a bounce that arrived after delivery (reason <see cref="EmailEventReasons.AsyncBounce"/>); null otherwise.</summary>
    public bool? Async { get; init; }

    /// <summary>The delivery attempt.</summary>
    public short? Attempt { get; init; }

    /// <summary>The receiving server.</summary>
    public string? RemoteHost { get; init; }

    /// <summary>The enhanced status code, such as <c>5.1.1</c>.</summary>
    public string? EnhancedCode { get; init; }

    /// <summary>Why it bounced or was deferred, such as <c>InvalidRecipient</c>.</summary>
    public string? Classification { get; init; }
}
