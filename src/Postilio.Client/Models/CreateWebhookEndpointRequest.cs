namespace Postilio;

/// <summary>A webhook endpoint to add.</summary>
public sealed class CreateWebhookEndpointRequest
{
    /// <summary>An <c>https://</c> URL on a public address.</summary>
    public required string Url { get; init; }

    /// <summary>The events to send; see <see cref="WebhookEventTypes"/>.</summary>
    public required IReadOnlyList<string> Events { get; init; }

    /// <summary>At most 200 characters.</summary>
    public string? Description { get; init; }

    /// <summary><see cref="WebhookModes.Live"/> (the default) or <see cref="WebhookModes.Test"/>.</summary>
    public string? Mode { get; init; }
}
