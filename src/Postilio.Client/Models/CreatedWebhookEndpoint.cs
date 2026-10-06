namespace Postilio;

/// <summary>A new webhook endpoint and its signing secret.</summary>
public sealed class CreatedWebhookEndpoint
{
    /// <summary>The endpoint.</summary>
    public WebhookEndpointResponse Endpoint { get; init; } = new();

    /// <summary>The signing secret, <c>whsec_…</c>. Shown this once: store it now.</summary>
    public string Secret { get; init; } = string.Empty;
}
