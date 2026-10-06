namespace Postilio;

/// <summary>A page of the suppression list, newest first.</summary>
public sealed class SuppressionList
{
    /// <summary>The suppressions on this page.</summary>
    public IReadOnlyList<SuppressionResponse> Data { get; init; } = [];

    /// <summary>Pass as <c>before</c> for the next page; null on the last page.</summary>
    public Guid? Next { get; init; }
}
