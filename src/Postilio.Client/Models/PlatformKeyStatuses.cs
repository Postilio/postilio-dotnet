namespace Postilio;

/// <summary>The statuses of the DKIM key record Postilio publishes for a domain (<see cref="PlatformKeyRecord"/>).</summary>
public static class PlatformKeyStatuses
{
    /// <summary>Found in DNS at the last check.</summary>
    public const string Published = "published";

    /// <summary>Postilio is publishing it, or it is not checked yet. Usually a minute.</summary>
    public const string Publishing = "publishing";

    /// <summary>Publishing it keeps failing; Postilio's team is alerted and it is retried. Nothing to do on your side.</summary>
    public const string Failed = "failed";

    /// <summary>This Postilio installation publishes these records by hand; its operator is alerted when one is missing.</summary>
    public const string Manual = "manual";
}
