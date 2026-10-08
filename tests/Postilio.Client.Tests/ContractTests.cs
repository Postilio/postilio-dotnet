using System.Text.Json.Nodes;

namespace Postilio.Client.Tests;

/// <summary>
/// Runs against a real Postilio with a test key, so nothing is delivered. Skipped unless POSTILIO_CONTRACT_BASE_ADDRESS,
/// POSTILIO_CONTRACT_API_KEY (<c>pk_test_</c>, scopes emails:send and emails:read) and POSTILIO_CONTRACT_FROM (an
/// address on a verified domain of the key's project) are set. See CONTRIBUTING.md.
/// </summary>
[Trait("Category", "Contract")]
public sealed class ContractTests
{
    private const string Delivered = "delivered@simulator.postilio.eu";
    private static readonly string? BaseAddress = Environment.GetEnvironmentVariable("POSTILIO_CONTRACT_BASE_ADDRESS");
    private static readonly string? ApiKey = Environment.GetEnvironmentVariable("POSTILIO_CONTRACT_API_KEY");
    private static readonly string? From = Environment.GetEnvironmentVariable("POSTILIO_CONTRACT_FROM");

    [Fact]
    public async Task Server_ServesTheSpecThisClientWasBuiltFrom()
    {
        var client = Client();
        using var http = new HttpClient { BaseAddress = new Uri(BaseAddress ?? string.Empty) };

        var live = JsonNode.Parse(await http.GetStringAsync(new Uri("openapi/v1.json", UriKind.Relative), TestContext.Current.CancellationToken));
        var copy = JsonNode.Parse(await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "spec", "openapi-v1.json"), TestContext.Current.CancellationToken));

        Assert.NotNull(client);
        Assert.True(JsonNode.DeepEquals(copy?["paths"], live?["paths"]), "The server's /v1 paths differ from spec/openapi-v1.json.");
        Assert.True(JsonNode.DeepEquals(copy?["components"], live?["components"]), "The server's /v1 schemas differ from spec/openapi-v1.json.");
    }

    [Fact]
    public async Task SendEmail_SameIdempotencyKey_IsAcceptedOnceAndFoundWithItsEvents()
    {
        var client = Client();
        var key = $"postilio-dotnet-contract-{Guid.NewGuid()}";
        var request = new SendEmailRequest { From = From ?? string.Empty, To = [Delivered], Subject = "Contract test", Text = "Sent by the .NET SDK's contract tests.", Tag = "sdk-contract" };

        var first = await client.SendEmailAsync(request, key, TestContext.Current.CancellationToken);
        var repeat = await client.SendEmailAsync(request, key, TestContext.Current.CancellationToken);
        var email = await client.GetEmailAsync(Assert.Single(first.Ids), TestContext.Current.CancellationToken);

        Assert.Equal(first.Ids, repeat.Ids);
        Assert.True(email.Test);
        Assert.Equal("sdk-contract", email.Tag);
        Assert.Equal(EmailStatuses.Delivered, email.Status);
        Assert.Contains(email.Events, e => e.Type == EmailStatuses.Delivered);
    }

    [Fact]
    public async Task SendEmail_SameIdempotencyKeyOtherBody_ThrowsConflict()
    {
        var client = Client();
        var key = $"postilio-dotnet-contract-{Guid.NewGuid()}";
        await client.SendEmailAsync(new SendEmailRequest { From = From ?? string.Empty, To = [Delivered], Subject = "One", Text = "One." }, key, TestContext.Current.CancellationToken);

        var error = await Assert.ThrowsAsync<PostilioConflictException>(() =>
            client.SendEmailAsync(new SendEmailRequest { From = From ?? string.Empty, To = [Delivered], Subject = "Two", Text = "Two." }, key, TestContext.Current.CancellationToken));

        Assert.Equal(PostilioErrorCodes.IdempotencyKeyReusedWithDifferentRequest, error.ErrorCode);
    }

    [Fact]
    public async Task SendEmail_TestKeyWithSendAt_IsSimulatedAtOnceSoCancelingConflicts()
    {
        var client = Client();
        var sent = await client.SendEmailAsync(new SendEmailRequest { From = From ?? string.Empty, To = [Delivered], Subject = "Scheduled", Text = "Simulated at once.", SendAt = DateTimeOffset.UtcNow.AddHours(1) }, TestContext.Current.CancellationToken);

        var error = await Assert.ThrowsAsync<PostilioConflictException>(() => client.CancelEmailAsync(Assert.Single(sent.Ids), TestContext.Current.CancellationToken));

        Assert.Equal(PostilioErrorCodes.EmailNotScheduled, error.ErrorCode);
    }

    [Fact]
    public async Task SendEmail_InvalidRequest_ThrowsValidationPerField()
    {
        var error = await Assert.ThrowsAsync<PostilioValidationException>(() =>
            Client().SendEmailAsync(new SendEmailRequest { From = From ?? string.Empty, To = [], Subject = "No one", Text = "Hi." }, TestContext.Current.CancellationToken));

        Assert.Contains("to", error.Errors.Keys);
    }

    [Fact]
    public async Task SendEmail_UnknownSenderDomain_ThrowsUnprocessable()
    {
        var error = await Assert.ThrowsAsync<PostilioUnprocessableException>(() =>
            Client().SendEmailAsync(new SendEmailRequest { From = "someone@not-a-domain-of-this-project.example", To = [Delivered], Subject = "Hi", Text = "Hi." }, TestContext.Current.CancellationToken));

        Assert.Equal(PostilioErrorCodes.UnverifiedSenderDomain, error.ErrorCode);
    }

    [Fact]
    public async Task GetEmail_Unknown_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<PostilioNotFoundException>(() => Client().GetEmailAsync(Guid.NewGuid(), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ListDomains_TestKey_ThrowsInsufficientScope()
    {
        var error = await Assert.ThrowsAsync<PostilioPermissionException>(() => Client().ListDomainsAsync(TestContext.Current.CancellationToken));

        Assert.Equal(PostilioErrorCodes.InsufficientScope, error.ErrorCode);
    }

    [Fact]
    public async Task AnyCall_RevokedOrUnknownKey_ThrowsInvalidApiKey()
    {
        Client();
        var stranger = new PostilioClient(new PostilioOptions { ApiKey = "pk_test_00000000000000000000000000000000", BaseAddress = new Uri(BaseAddress ?? string.Empty) });

        var error = await Assert.ThrowsAsync<PostilioAuthenticationException>(() => stranger.GetEmailAsync(Guid.NewGuid(), TestContext.Current.CancellationToken));

        Assert.Equal(PostilioErrorCodes.InvalidApiKey, error.ErrorCode);
    }

    private static PostilioClient Client()
    {
        Assert.SkipWhen(BaseAddress is null || ApiKey is null || From is null,
            "Set POSTILIO_CONTRACT_BASE_ADDRESS, POSTILIO_CONTRACT_API_KEY and POSTILIO_CONTRACT_FROM to run the contract tests.");
        // A live key would deliver real mail.
        Assert.StartsWith("pk_test_", ApiKey, StringComparison.Ordinal);
        return new PostilioClient(new PostilioOptions { ApiKey = ApiKey ?? string.Empty, BaseAddress = new Uri(BaseAddress ?? string.Empty) });
    }
}
