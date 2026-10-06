namespace Postilio;

/// <summary>The project's webhook endpoints.</summary>
public sealed class WebhookEndpointList
{
    /// <summary>The endpoints.</summary>
    public IReadOnlyList<WebhookEndpointResponse> Data { get; init; } = [];
}
