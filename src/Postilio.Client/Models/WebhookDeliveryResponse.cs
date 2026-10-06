namespace Postilio;

/// <summary>A delivery of an event to a webhook endpoint, with its attempts.</summary>
public sealed class WebhookDeliveryResponse
{
    /// <summary>The delivery id.</summary>
    public Guid Id { get; init; }

    /// <summary>The event id: the <c>webhook-id</c> header.</summary>
    public Guid EventId { get; init; }

    /// <summary>The event type, such as <c>email.delivered.v1</c>.</summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>The message the event is about; null for a test event.</summary>
    public Guid? EmailId { get; init; }

    /// <summary>The message's recipient.</summary>
    public string? To { get; init; }

    /// <summary>See <see cref="WebhookDeliveryStatuses"/>.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>The number of attempts so far.</summary>
    public int Attempts { get; init; }

    /// <summary>When it was last attempted.</summary>
    public DateTimeOffset? LastAttemptAt { get; init; }

    /// <summary>The status code of the last attempt, if the endpoint answered.</summary>
    public int? LastStatusCode { get; init; }

    /// <summary>Why the last attempt failed.</summary>
    public string? LastError { get; init; }

    /// <summary>How long the last attempt took.</summary>
    public int? LastDurationMs { get; init; }

    /// <summary>The start of the endpoint's answer.</summary>
    public string? ResponseSnippet { get; init; }

    /// <summary>When it is tried again.</summary>
    public DateTimeOffset? NextAttemptAt { get; init; }

    /// <summary>When the delivery was created.</summary>
    public DateTimeOffset CreatedAt { get; init; }
}
