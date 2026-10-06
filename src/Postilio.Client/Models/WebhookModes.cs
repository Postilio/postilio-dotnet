namespace Postilio;

/// <summary>Which messages' events a webhook endpoint gets.</summary>
public static class WebhookModes
{
    /// <summary>Messages sent with a live key.</summary>
    public const string Live = "live";

    /// <summary>Messages sent with a test key.</summary>
    public const string Test = "test";
}
