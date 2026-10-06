namespace Postilio;

/// <summary>Why an address is on the suppression list.</summary>
public static class SuppressionReasons
{
    /// <summary>A receiving server refused it for good.</summary>
    public const string HardBounce = "hard_bounce";

    /// <summary>The recipient marked a message as spam.</summary>
    public const string Complaint = "complaint";

    /// <summary>Added by hand.</summary>
    public const string Manual = "manual";
}
