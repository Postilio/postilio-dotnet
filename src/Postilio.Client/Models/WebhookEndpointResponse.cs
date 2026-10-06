namespace Postilio;

/// <summary>A webhook endpoint.</summary>
public sealed class WebhookEndpointResponse
{
    /// <summary>The endpoint id.</summary>
    public Guid Id { get; init; }

    /// <summary>Where deliveries go.</summary>
    public string Url { get; init; } = string.Empty;

    /// <summary>The description, if any.</summary>
    public string? Description { get; init; }

    /// <summary>The events it gets; see <see cref="WebhookEventTypes"/>.</summary>
    public IReadOnlyList<string> Events { get; init; } = [];

    /// <summary>See <see cref="WebhookModes"/>.</summary>
    public string Mode { get; init; } = string.Empty;

    /// <summary>A paused endpoint gets no new events.</summary>
    public bool Paused { get; init; }

    /// <summary>Why it was paused.</summary>
    public string? PauseReason { get; init; }

    /// <summary>When it was paused.</summary>
    public DateTimeOffset? PausedAt { get; init; }

    /// <summary>Since when every delivery has failed.</summary>
    public DateTimeOffset? FailingSince { get; init; }

    /// <summary>The last characters of the signing secret, to tell secrets apart.</summary>
    public string SecretHint { get; init; } = string.Empty;

    /// <summary>Until when the previous secret still signs, after a rotation.</summary>
    public DateTimeOffset? PreviousSecretExpiresAt { get; init; }

    /// <summary>When it was added.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>The most recent delivery attempt, if any.</summary>
    public WebhookLastDelivery? LastDelivery { get; init; }
}
