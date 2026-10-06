namespace Postilio;

/// <summary>The statuses of a message and the types of its events. New ones may be added; handle unknown values.</summary>
public static class EmailStatuses
{
    /// <summary>Stored, waiting to be handed to the mail server.</summary>
    public const string Accepted = "accepted";

    /// <summary>Handed to the mail server.</summary>
    public const string Queued = "queued";

    /// <summary>The receiving server accepted it.</summary>
    public const string Delivered = "delivered";

    /// <summary>Refused for now; it is tried again.</summary>
    public const string Deferred = "deferred";

    /// <summary>Refused for good.</summary>
    public const string Bounced = "bounced";

    /// <summary>Not delivered within a day.</summary>
    public const string Expired = "expired";

    /// <summary>The recipient marked it as spam.</summary>
    public const string Complained = "complained";

    /// <summary>Not sent: the address is on the suppression list.</summary>
    public const string Suppressed = "suppressed";
}
