namespace Postilio;

/// <summary>The project's sending domains.</summary>
public sealed class DomainList
{
    /// <summary>The domains.</summary>
    public IReadOnlyList<DomainResponse> Data { get; init; } = [];
}
