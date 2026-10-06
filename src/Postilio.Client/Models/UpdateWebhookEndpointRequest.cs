namespace Postilio;

/// <summary>Changes to a webhook endpoint; what you leave null stays as it is.</summary>
public sealed class UpdateWebhookEndpointRequest
{
    /// <summary>A new <c>https://</c> URL.</summary>
    public string? Url { get; init; }

    /// <summary>The new set of events.</summary>
    public IReadOnlyList<string>? Events { get; init; }

    /// <summary>A new description; an empty string removes it.</summary>
    public string? Description { get; init; }

    /// <summary>True pauses the endpoint, false resumes it.</summary>
    public bool? Paused { get; init; }
}
