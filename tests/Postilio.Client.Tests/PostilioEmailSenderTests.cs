using System.Net;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Postilio.AspNetCore;

namespace Postilio.Client.Tests;

public sealed class PostilioEmailSenderTests
{
    private const string Sent = """{"ids":["01a10ce5-a09a-788b-9243-d7c9c59773d2"],"suppressed":[]}""";
    private readonly StubHandler _http = new();

    [Fact]
    public async Task SendConfirmationLinkAsync_IdentityLink_IsSentAsHtmlAndPlainText()
    {
        var sender = Sender(o => o.From = "Acme <no-reply@mail.example.com>");
        _http.Answer(HttpStatusCode.Accepted, Sent);

        // Identity passes the link HTML-encoded.
        await sender.SendConfirmationLinkAsync(new IdentityUser(), "ada@example.com", "https://example.com/confirm?userId=1&amp;code=abc");

        var body = JsonNode.Parse(_http.Requests[0].Body ?? string.Empty);
        Assert.Equal("Acme <no-reply@mail.example.com>", (string?)body?["from"]);
        Assert.Equal("ada@example.com", (string?)body?["to"]?[0]);
        Assert.Equal("Confirm your email", (string?)body?["subject"]);
        Assert.Equal("Please confirm your account by <a href='https://example.com/confirm?userId=1&amp;code=abc'>clicking here</a>.", (string?)body?["html"]);
        Assert.Equal("Please confirm your account by opening this link: https://example.com/confirm?userId=1&code=abc", (string?)body?["text"]);
        Assert.Equal("identity", (string?)body?["tag"]);
    }

    [Fact]
    public async Task SendPasswordResetLinkAsync_IdentityLink_IsSent()
    {
        var sender = Sender(o => o.From = "no-reply@mail.example.com");
        _http.Answer(HttpStatusCode.Accepted, Sent);

        await sender.SendPasswordResetLinkAsync(new IdentityUser(), "ada@example.com", "https://example.com/reset?code=abc");

        var body = JsonNode.Parse(_http.Requests[0].Body ?? string.Empty);
        Assert.Equal("Reset your password", (string?)body?["subject"]);
        Assert.Equal("Please reset your password by <a href='https://example.com/reset?code=abc'>clicking here</a>.", (string?)body?["html"]);
    }

    [Fact]
    public async Task SendPasswordResetCodeAsync_Code_IsHtmlEncodedInTheHtmlBody()
    {
        var sender = Sender(o =>
        {
            o.From = "no-reply@mail.example.com";
            o.Tag = null;
        });
        _http.Answer(HttpStatusCode.Accepted, Sent);

        await sender.SendPasswordResetCodeAsync(new IdentityUser(), "ada@example.com", "<b>123</b>");

        var body = JsonNode.Parse(_http.Requests[0].Body ?? string.Empty);
        Assert.Equal("Please reset your password using the following code: &lt;b&gt;123&lt;/b&gt;", (string?)body?["html"]);
        Assert.Equal("Please reset your password using the following code: <b>123</b>", (string?)body?["text"]);
        Assert.Null(body?["tag"]);
    }

    [Fact]
    public void AddPostilioEmailSender_NoFrom_FailsWhenTheSenderIsResolved()
    {
        Assert.Throws<OptionsValidationException>(() => Sender(_ => { }));
    }

    private IEmailSender<IdentityUser> Sender(Action<PostilioEmailSenderOptions> configure)
    {
        var services = new ServiceCollection();
        services.AddPostilio(o => o.ApiKey = "pk_test_abcdefghijklmnopqrstuvwxyz012345").ConfigurePrimaryHttpMessageHandler(() => _http);
        services.AddPostilioEmailSender<IdentityUser>(configure);
        return services.BuildServiceProvider().GetRequiredService<IEmailSender<IdentityUser>>();
    }
}
