namespace Postilio;

/// <summary>Why an address comes off the suppression list.</summary>
public sealed class RemoveSuppressionRequest
{
    /// <summary>10 to 500 characters; required to remove a complaint, which Postilio keeps.</summary>
    public string? Reason { get; init; }
}
