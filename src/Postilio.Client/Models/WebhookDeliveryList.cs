namespace Postilio;

/// <summary>A page of an endpoint's deliveries, newest first.</summary>
public sealed class WebhookDeliveryList
{
    /// <summary>The deliveries on this page.</summary>
    public IReadOnlyList<WebhookDeliveryResponse> Data { get; init; } = [];

    /// <summary>Pass as <c>before</c> for the next page; null on the last page.</summary>
    public Guid? Next { get; init; }
}
