namespace Postilio;

/// <summary>The key's project in one UTC month, and where its organization stands against its plan.</summary>
public sealed class ApiUsage
{
    /// <summary>The month, <c>yyyy-MM</c>.</summary>
    public string Month { get; init; } = string.Empty;

    /// <summary>True when the month is closed: its numbers no longer change.</summary>
    public bool Final { get; init; }

    /// <summary>The start of the next month, 00:00 UTC.</summary>
    public DateTimeOffset ResetsAt { get; init; }

    /// <summary>The organization's plan and state, without its numbers.</summary>
    public ApiUsageOrganization Organization { get; init; } = new();

    /// <summary>The project's numbers: what is billed.</summary>
    public ApiProjectUsage Project { get; init; } = new();

    /// <summary>The calling key's own month in its project; informative.</summary>
    public ApiKeyMonthUsage ApiKey { get; init; } = new();
}
