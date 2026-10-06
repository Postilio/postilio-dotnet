namespace Postilio;

/// <summary>A message and its events.</summary>
public sealed class EmailDetails
{
    /// <summary>The message id.</summary>
    public Guid Id { get; init; }

    /// <summary>The current status; see <see cref="EmailStatuses"/>.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>The sender's address.</summary>
    public string From { get; init; } = string.Empty;

    /// <summary>The recipient.</summary>
    public string To { get; init; } = string.Empty;

    /// <summary>Null when the project does not keep subjects.</summary>
    public string? Subject { get; init; }

    /// <summary>The tag it was sent with.</summary>
    public string? Tag { get; init; }

    /// <summary>When Postilio accepted it.</summary>
    public DateTimeOffset AcceptedAt { get; init; }

    /// <summary>True for a message sent with a test key.</summary>
    public bool Test { get; init; }

    /// <summary>How it was submitted: <c>api</c> or <c>smtp</c>.</summary>
    public string Via { get; init; } = string.Empty;

    /// <summary>What happened to it, oldest first.</summary>
    public IReadOnlyList<EmailEvent> Events { get; init; } = [];
}
