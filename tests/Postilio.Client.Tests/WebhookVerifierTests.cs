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

    [Fact]
    public void Parse_DocsExample_IsReadIntoTypedProperties()
    {
        var bounced = WebhookEvent.Parse("""
            {
              "type": "email.bounced.v1",
              "timestamp": "2026-10-03T14:07:45.102+00:00",
              "data": {
                "emailId": "0199a7c4-5a1e-7d2b-9c41-6f3e0b8a2d17", "projectId": "0199a1b2-0000-7000-8000-000000000001",
                "to": "ada.lovelace@example.com", "tag": "sign-in", "test": false, "event": "bounced",
                "occurredAt": "2026-10-03T14:07:45.102+00:00", "attempt": 1, "smtpCode": 550, "enhancedCode": "5.1.1",
                "classification": "InvalidRecipient", "response": "550 5.1.1 The email account that you tried to reach does not exist",
                "remoteHost": "mx.example.com", "aFieldAddedLater": true
              }
            }
            """);

        Assert.Equal("email.bounced.v1", bounced.Type);
        Assert.Equal(Guid.Parse("0199a7c4-5a1e-7d2b-9c41-6f3e0b8a2d17"), bounced.Data.EmailId);
        Assert.Equal(EmailStatuses.Bounced, bounced.Data.Event);
        Assert.Equal(550, bounced.Data.SmtpCode);
        Assert.Equal(new DateTimeOffset(2026, 10, 3, 14, 7, 45, 102, TimeSpan.Zero), bounced.Data.OccurredAt);
    }

    private static FakeTimeProvider ClockAt(long unixSeconds) => new(DateTimeOffset.FromUnixTimeSeconds(unixSeconds));
}
