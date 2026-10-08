namespace Postilio;

/// <summary>A project's live messages in a month, one per recipient.</summary>
public sealed class ApiProjectUsage
{
    /// <summary>The project id.</summary>
    public Guid Id { get; init; }

    /// <summary>Accepted minus suppressed: what is billed and counts towards the monthly limit.</summary>
    public long Billable { get; init; }

    /// <summary>Messages accepted.</summary>
    public long Accepted { get; init; }

    /// <summary>Accepted but never sent, because the address is suppressed.</summary>
    public long Suppressed { get; init; }

    /// <summary>Delivered in the month.</summary>
    public long Delivered { get; init; }

    /// <summary>Bounced in the month.</summary>
    public long Bounced { get; init; }

    /// <summary>Complaints in the month.</summary>
    public long Complained { get; init; }
}
