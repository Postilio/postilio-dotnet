namespace Postilio;

/// <summary>A sending domain and the DNS records it needs.</summary>
public sealed class DomainResponse
{
    /// <summary>The domain id.</summary>
    public Guid Id { get; init; }

    /// <summary>The domain name, lowercase.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>See <see cref="DomainStatuses"/>.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>When the records were last checked; null before the first check.</summary>
    public DateTimeOffset? CheckedAt { get; init; }

    /// <summary>The records to create, each with its own status.</summary>
    public IReadOnlyList<DnsRecord> Records { get; init; } = [];

    /// <summary>When it was added.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Who added it in the portal; null when added with an API key.</summary>
    public string? AddedBy { get; init; }

    /// <summary>Since when a record is missing; the domain keeps sending for 72 hours after this.</summary>
    public DateTimeOffset? FailingSince { get; init; }

    /// <summary>Live messages sent from it in the last 30 days.</summary>
    public int Sent30d { get; init; }
}
