namespace Postilio;

/// <summary>A DNS record a sending domain needs.</summary>
public sealed class DnsRecord
{
    /// <summary>The record type, such as <c>CNAME</c>.</summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>The record's name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>The record's value.</summary>
    public string Value { get; init; } = string.Empty;

    /// <summary><c>found</c>, <c>missing</c>, or <c>unknown</c> before the first check.</summary>
    public string Status { get; init; } = string.Empty;
}
