namespace Postilio;

/// <summary>The most recent delivery attempt to a webhook endpoint.</summary>
public sealed class WebhookLastDelivery
{
    /// <summary>When it was attempted.</summary>
    public DateTimeOffset At { get; init; }

    /// <summary>The status code the endpoint answered, if it answered.</summary>
    public int? StatusCode { get; init; }

    /// <summary>See <see cref="WebhookDeliveryStatuses"/>.</summary>
    public string Status { get; init; } = string.Empty;
}
