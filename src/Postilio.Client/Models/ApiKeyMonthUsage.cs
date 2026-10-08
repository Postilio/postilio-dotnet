namespace Postilio;

/// <summary>
/// The calling key's messages in the month, by the UTC day they were accepted and their status now, so a late bounce
/// counts in the month the message was accepted in. Informative; the project's numbers are what is billed.
/// </summary>
public sealed class ApiKeyMonthUsage
{
    /// <summary>The key making the call.</summary>
    public Guid Id { get; init; }

    /// <summary>Messages the key had accepted, one per recipient.</summary>
    public long Accepted { get; init; }

    /// <summary>Accepted minus suppressed.</summary>
    public long Sent { get; init; }

    /// <summary>Accepted but never sent, because the address is suppressed.</summary>
    public long Suppressed { get; init; }

    /// <summary>Delivered now; one with a complaint after delivery counts as complained instead.</summary>
    public long Delivered { get; init; }

    /// <summary>Bounced.</summary>
    public long Bounced { get; init; }

    /// <summary>Complained.</summary>
    public long Complained { get; init; }
}
