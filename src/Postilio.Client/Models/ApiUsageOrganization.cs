namespace Postilio;

/// <summary>Where the organization stands against its plan's monthly limit.</summary>
public sealed class ApiUsageOrganization
{
    /// <summary>The code of the plan the organization has now.</summary>
    public string? Plan { get; init; }

    /// <summary><c>ok</c>, <c>warning</c>, <c>over</c> or <c>capped</c> for the current month; null for another one.</summary>
    public string? State { get; init; }
}
