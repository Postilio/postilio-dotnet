namespace Postilio;

/// <summary>The domain's DMARC record as Postilio found it at the last check. Advice only: sending never depends on it.</summary>
public sealed class DmarcCheck
{
    /// <summary><c>missing</c>, <c>invalid</c> (receivers ignore it), <c>monitoring</c> (<c>p=none</c>) or <c>enforced</c> (<c>p=quarantine</c> or <c>p=reject</c>).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Where the record was found: the domain itself or a parent; null while missing.</summary>
    public string? PolicyDomain { get; init; }

    /// <summary>The DMARC records found there, as published.</summary>
    public IReadOnlyList<string> Records { get; init; } = [];

    /// <summary>
    /// Why it is invalid (<c>multiple_records</c>, <c>version</c>, <c>no_policy</c>, <c>syntax</c>), or advice on a valid
    /// one (<c>no_reports</c>, <c>spf_strict</c>).
    /// </summary>
    public IReadOnlyList<string> Issues { get; init; } = [];
}
