using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Postilio.Webhooks;

/// <summary>
/// Verifies the signature of a Postilio webhook delivery (the Standard Webhooks scheme).
/// Create one per signing secret and reuse it; it is thread-safe.
/// </summary>
public sealed class WebhookVerifier
{
    private const string SecretPrefix = "whsec_";
    private const string SignatureVersion = "v1";
    private readonly byte[] _key;
    private readonly TimeProvider _time;

    /// <param name="secret">The endpoint's signing secret, <c>whsec_…</c>, as Postilio showed it when the endpoint was created.</param>
    /// <param name="timeProvider">The clock to check the timestamp against; the system clock by default.</param>
    /// <exception cref="ArgumentException">The secret is empty or not base64 after the <c>whsec_</c> prefix.</exception>
    public WebhookVerifier(string secret, TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(secret);
        var encoded = secret.StartsWith(SecretPrefix, StringComparison.Ordinal) ? secret[SecretPrefix.Length..] : secret;
        var key = new byte[encoded.Length];
        if (encoded.Length == 0 || !Convert.TryFromBase64String(encoded, key, out var length))
        {
            throw new ArgumentException("A webhook secret is whsec_ followed by base64.", nameof(secret));
        }
        _key = key[..length];
        _time = timeProvider ?? TimeProvider.System;
    }

    /// <summary>How far the <c>webhook-timestamp</c> may be from now, either way: 5 minutes by default.</summary>
    public TimeSpan Tolerance { get; init; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// True when one of the signatures matches and the timestamp is within <see cref="Tolerance"/>, which turns away a
    /// replayed request. Pass the raw body as received, not re-serialized JSON.
    /// </summary>
    /// <param name="id">The <c>webhook-id</c> header.</param>
    /// <param name="timestamp">The <c>webhook-timestamp</c> header.</param>
    /// <param name="signatureHeader">The <c>webhook-signature</c> header; during a secret rotation it holds two signatures.</param>
    /// <param name="body">The raw request body.</param>
    public bool Verify(string? id, string? timestamp, string? signatureHeader, string body)
    {
        ArgumentNullException.ThrowIfNull(body);
        if (string.IsNullOrEmpty(signatureHeader)
            || !long.TryParse(timestamp, NumberStyles.None, CultureInfo.InvariantCulture, out var seconds)
            // In seconds, not as a DateTimeOffset: a forged timestamp beyond year 9999 must be refused, not throw.
            || Math.Abs(_time.GetUtcNow().ToUnixTimeSeconds() - seconds) > Tolerance.TotalSeconds)
        {
            return false;
        }
        var expected = HMACSHA256.HashData(_key, Encoding.UTF8.GetBytes($"{id}.{timestamp}.{body}"));
        var given = new byte[expected.Length];
        foreach (var signature in signatureHeader.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (signature.Split(',', 2) is [SignatureVersion, var value]
                && Convert.TryFromBase64String(value, given, out var length) && length == given.Length
                && CryptographicOperations.FixedTimeEquals(given, expected))
            {
                return true;
            }
        }
        return false;
    }
}
