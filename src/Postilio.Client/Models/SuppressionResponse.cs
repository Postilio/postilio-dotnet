namespace Postilio;

/// <summary>An address on the suppression list.</summary>
public sealed class SuppressionResponse
{
    /// <summary>The suppression id.</summary>
    public Guid Id { get; init; }

    /// <summary>The address.</summary>
    public string Address { get; init; } = string.Empty;

    /// <summary>See <see cref="SuppressionReasons"/>.</summary>
    public string Reason { get; init; } = string.Empty;

    /// <summary>The receiving server's reply that put it on the list, when there was one.</summary>
    public string? Detail { get; init; }

    /// <summary>The message that put it on the list, when there was one.</summary>
    public Guid? SourceMessageId { get; init; }

    /// <summary>When it was added.</summary>
    public DateTimeOffset CreatedAt { get; init; }
}
