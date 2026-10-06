namespace Postilio;

/// <summary>A webhook endpoint's new signing secret.</summary>
public sealed class RotatedWebhookSecret
{
    /// <summary>The new signing secret, <c>whsec_…</c>. Shown this once: store it now.</summary>
    public string Secret { get; init; } = string.Empty;

    /// <summary>Until when the previous secret keeps signing too.</summary>
    public DateTimeOffset PreviousSecretExpiresAt { get; init; }
}
