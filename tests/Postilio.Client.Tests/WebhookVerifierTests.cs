using System.Globalization;
using Microsoft.Extensions.Time.Testing;
using Postilio.Webhooks;

namespace Postilio.Client.Tests;

public sealed class WebhookVerifierTests
{
    // The test vector of the Standard Webhooks specification.
    private const string Secret = "whsec_MfKQ9r8GKYqrTwjUPD8ILPZIo2LaLaSw";
    private const string Id = "msg_p5jXN8AQM9LWM0D4loKWxJek";
    private const long Timestamp = 1614265330;
    private const string Body = """{"test": 2432232314}""";
    private const string Signature = "v1,g0hM9SsE+OTPJTGt/tmIKtSyZlE3uFJELVlNIOLJ1OE=";
    private const string OtherSecret = "whsec_c2VjcmV0LW9mLWEtcm90YXRlZC1lbmRwb2ludA==";

    [Theory]
    [InlineData(Secret, Id, "1614265330", Body, Signature, 0, true)]
    [InlineData("MfKQ9r8GKYqrTwjUPD8ILPZIo2LaLaSw", Id, "1614265330", Body, Signature, 0, true)]
    [InlineData(Secret, Id, "1614265330", """{"test": 2432232315}""", Signature, 0, false)]
    [InlineData(Secret, "msg_other", "1614265330", Body, Signature, 0, false)]
    [InlineData(Secret, Id, "1614265331", Body, Signature, 0, false)]
    [InlineData(OtherSecret, Id, "1614265330", Body, Signature, 0, false)]
    [InlineData(Secret, Id, "1614265330", Body, Signature, 300, true)]
    [InlineData(Secret, Id, "1614265330", Body, Signature, -300, true)]
    [InlineData(Secret, Id, "1614265330", Body, Signature, 301, false)]
    [InlineData(Secret, Id, "1614265330", Body, Signature, -301, false)]
    [InlineData(Secret, Id, "not-a-number", Body, Signature, 0, false)]
    [InlineData(Secret, Id, "1614265330", Body, "v2,g0hM9SsE+OTPJTGt/tmIKtSyZlE3uFJELVlNIOLJ1OE=", 0, false)]
    [InlineData(Secret, Id, "1614265330", Body, "g0hM9SsE+OTPJTGt/tmIKtSyZlE3uFJELVlNIOLJ1OE=", 0, false)]
    [InlineData(Secret, Id, "1614265330", Body, "v1,not-base64!", 0, false)]
    [InlineData(Secret, Id, "1614265330", Body, "v1,g0hM9SsE+OTPJTGt/tmIKtSyZlE3uFJELVlNIOLJ1A==", 0, false)]
    [InlineData(Secret, Id, "1614265330", Body, "v1,g0hM9SsE+OTPJTGt/tmIKtSyZlE3uFJELVlNIOLJ1OE=AAAA", 0, false)]
    [InlineData(Secret, Id, "1614265330", Body, "v1,AAAA v1,g0hM9SsE+OTPJTGt/tmIKtSyZlE3uFJELVlNIOLJ1OE=", 0, true)]
    [InlineData(Secret, Id, "1614265330", Body, "", 0, false)]
    [InlineData(Secret, "", "1614265330", Body, Signature, 0, false)]
    public void Verify_StandardWebhooksTestVector_AcceptsOnlyAFreshMatchingSignature(
        string secret, string id, string timestamp, string body, string signature, int secondsLater, bool valid)
    {
        var verifier = new WebhookVerifier(secret, ClockAt(Timestamp + secondsLater));

        Assert.Equal(valid, verifier.Verify(id, timestamp, signature, body));
    }

    [Fact]
    public void Verify_MissingHeaders_IsFalse()
    {
        var verifier = new WebhookVerifier(Secret, ClockAt(Timestamp));

        Assert.False(verifier.Verify(null, null, null, Body));
    }

    [Fact]
    public void Verify_WiderTolerance_AcceptsAnOlderTimestamp()
    {
        var verifier = new WebhookVerifier(Secret, ClockAt(Timestamp + 600)) { Tolerance = TimeSpan.FromMinutes(10) };

        Assert.True(verifier.Verify(Id, Timestamp.ToString(CultureInfo.InvariantCulture), Signature, Body));
    }

    [Theory]
    [InlineData("")]
    [InlineData("whsec_")]
    [InlineData("whsec_not base64")]
    public void Constructor_SecretThatIsNotBase64_Throws(string secret)
    {
        Assert.Throws<ArgumentException>(() => new WebhookVerifier(secret));
    }

    private static FakeTimeProvider ClockAt(long unixSeconds) => new(DateTimeOffset.FromUnixTimeSeconds(unixSeconds));
}
