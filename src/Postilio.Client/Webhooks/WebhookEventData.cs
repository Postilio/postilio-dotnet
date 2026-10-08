namespace Postilio.Webhooks;

/// <summary>The data of a webhook event. Never holds message content.</summary>
public sealed class WebhookEventData
{
    /// <summary>The message the event is about.</summary>
    public Guid? EmailId { get; init; }

    /// <summary>The project.</summary>
    public Guid? ProjectId { get; init; }

    /// <summary>For a test event: the endpoint it was sent to.</summary>
    public Guid? EndpointId { get; init; }

    /// <summary>The recipient.</summary>
    public string? To { get; init; }

    /// <summary>The tag the message was sent with.</summary>
    public string? Tag { get; init; }

    /// <summary>True for a message sent with a test key, and for a test event.</summary>
    public bool Test { get; init; }

    /// <summary>The event; see <see cref="EmailStatuses"/>.</summary>
    public string? Event { get; init; }

    /// <summary>When it happened: order events by this, they can arrive out of order.</summary>
    public DateTimeOffset? OccurredAt { get; init; }

    /// <summary>The delivery attempt.</summary>
    public int? Attempt { get; init; }

    /// <summary>The receiving server's SMTP reply code.</summary>
    public int? SmtpCode { get; init; }

    /// <summary>The enhanced status code, such as <c>5.1.1</c>.</summary>
    public string? EnhancedCode { get; init; }

    /// <summary>Why it bounced or was deferred, such as <c>InvalidRecipient</c>.</summary>
    public string? Classification { get; init; }

    /// <summary>The receiving server's reply.</summary>
    public string? Response { get; init; }

    /// <summary>Why it was delayed, failed or canceled; see <see cref="EmailEventReasons"/>.</summary>
    public string? Reason { get; init; }

    /// <summary>For a scheduled message: when it is to go out.</summary>
    public DateTimeOffset? SendAt { get; init; }

    /// <summary>The receiving server.</summary>
    public string? RemoteHost { get; init; }
}
