namespace Postilio;

/// <summary>The statuses of a webhook delivery.</summary>
public static class WebhookDeliveryStatuses
{
    /// <summary>Not delivered yet; it is retried.</summary>
    public const string Pending = "pending";

    /// <summary>The endpoint answered 2xx.</summary>
    public const string Delivered = "delivered";

    /// <summary>No attempt succeeded within 3 days.</summary>
    public const string Failed = "failed";
}
