namespace Postilio;

/// <summary>An address to put on the suppression list.</summary>
public sealed class CreateSuppressionRequest
{
    /// <summary>The address.</summary>
    public required string Address { get; init; }
}
