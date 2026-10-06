namespace Postilio;

/// <summary>Settings of a <see cref="PostilioClient"/>, usually bound from the <c>Postilio</c> configuration section.</summary>
public sealed class PostilioOptions
{
    /// <summary>The API key, <c>pk_live_…</c> or <c>pk_test_…</c>. Keep it in a secret store; the client never logs it.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>The API's address; <c>https://api.postilio.eu/</c> by default.</summary>
    public Uri BaseAddress { get; set; } = new("https://api.postilio.eu/");

    /// <summary>
    /// How often a failed request is tried again: 2 by default, 0 turns retries off. A 429 is retried for every call;
    /// a connection failure or a 408, 500, 502, 503 or 504 only for a GET or a send, which carries an Idempotency-Key.
    /// </summary>
    public int MaxRetries { get; set; } = 2;

    /// <summary>The longest wait before a retry: 30 seconds by default. A longer Retry-After throws at once instead.</summary>
    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(30);

    /// <inheritdoc />
    public override string ToString() => $"PostilioOptions {{ BaseAddress = {BaseAddress}, MaxRetries = {MaxRetries}, MaxRetryDelay = {MaxRetryDelay} }}";

    internal string? Problem() =>
        !ApiKey.StartsWith("pk_", StringComparison.Ordinal) ? "PostilioOptions.ApiKey must be an API key: pk_live_… or pk_test_…."
        : BaseAddress is not { IsAbsoluteUri: true } ? "PostilioOptions.BaseAddress must be an absolute URI."
        : MaxRetries < 0 ? "PostilioOptions.MaxRetries must be 0 or more."
        : MaxRetryDelay < TimeSpan.Zero ? "PostilioOptions.MaxRetryDelay must be zero or more."
        : null;
}
