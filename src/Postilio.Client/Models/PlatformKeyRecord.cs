namespace Postilio;

/// <summary>
/// The DKIM key record Postilio publishes itself, the one the domain's DKIM CNAME points to. Not one for you to create: a
/// pending domain whose own records are found waits for this one.
/// </summary>
public sealed class PlatformKeyRecord
{
    /// <summary>The record type, such as <c>TXT</c>.</summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>The record's name, under <c>dkim.postilio.eu</c>.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>See <see cref="PlatformKeyStatuses"/>.</summary>
    public string Status { get; init; } = string.Empty;
}
