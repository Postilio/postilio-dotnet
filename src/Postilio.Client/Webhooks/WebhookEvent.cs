using System.Text.Json;
using Postilio.Http;

namespace Postilio.Webhooks;

/// <summary>The body of a webhook delivery. Verify it with <see cref="WebhookVerifier"/> before you parse it.</summary>
public sealed class WebhookEvent
{
    /// <summary>The versioned event type, such as <c>email.delivered.v1</c> or <c>webhook.test.v1</c>.</summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>When the delivery was created.</summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>What happened. Fields without a value are null.</summary>
    public WebhookEventData Data { get; init; } = new();

    /// <summary>Reads a delivery's body; unknown fields are ignored.</summary>
    /// <exception cref="JsonException">The body is not a webhook event.</exception>
    public static WebhookEvent Parse(string body) =>
        JsonSerializer.Deserialize(body, PostilioJsonContext.Default.WebhookEvent) ?? throw new JsonException("The body is not a webhook event.");
}
