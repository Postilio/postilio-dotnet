using System.Globalization;
using System.Net;
using System.Reflection;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Postilio.Http;

namespace Postilio;

/// <summary>
/// The Postilio API (<c>/v1</c>). Thread-safe: create one and reuse it, or register it with
/// <c>services.AddPostilio(...)</c>. Every method throws a <see cref="PostilioException"/> for an error answer.
/// </summary>
public sealed class PostilioClient
{
    // One connection pool for every client made without DI; recycled so DNS changes are picked up.
    private static readonly SocketsHttpHandler SharedHandler = new() { PooledConnectionLifetime = TimeSpan.FromMinutes(2) };
    private static readonly ProductInfoHeaderValue UserAgent = new("postilio-dotnet",
        typeof(PostilioClient).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0] ?? "0");
    private readonly HttpClient _http;
    private readonly Uri _baseAddress;
    private readonly string _apiKey;

    /// <summary>A client with this API key and the default settings.</summary>
    public PostilioClient(string apiKey)
        : this(new PostilioOptions { ApiKey = apiKey })
    {
    }

    /// <summary>A client with these settings, retries included.</summary>
    public PostilioClient(PostilioOptions options)
        : this(options, SharedHandler, (wait, ct) => Task.Delay(wait, ct))
    {
    }

    /// <summary>
    /// A client on an <see cref="HttpClient"/> you manage, such as one from <c>IHttpClientFactory</c>. The client is
    /// left as it is: the address and the API key go on each request. Retries come from the handler <c>AddPostilio</c> adds.
    /// </summary>
    public PostilioClient(HttpClient httpClient, PostilioOptions options)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);
        if (options.Problem() is { } problem)
        {
            throw new ArgumentException(problem, nameof(options));
        }
        _http = httpClient;
        _baseAddress = options.BaseAddress.AbsoluteUri.EndsWith('/') ? options.BaseAddress : new Uri(options.BaseAddress.AbsoluteUri + "/");
        _apiKey = options.ApiKey;
    }

    internal PostilioClient(PostilioOptions options, HttpMessageHandler handler, Func<TimeSpan, CancellationToken, Task> delay)
        : this(new HttpClient(new RetryHandler(() => options, delay) { InnerHandler = handler }, disposeHandler: false), options)
    {
    }

    /// <summary>
    /// Sends an email: one message per recipient. Needs the <c>emails:send</c> scope. With a test key nothing is
    /// delivered. The client makes an Idempotency-Key per call, so its own retries never send twice.
    /// </summary>
    public Task<SendEmailResponse> SendEmailAsync(SendEmailRequest request, CancellationToken cancellationToken = default) =>
        SendEmailAsync(request, Guid.NewGuid().ToString(), cancellationToken);

    /// <summary>
    /// Sends an email with your own Idempotency-Key (1 to 256 characters), such as an order number: a repeat within 24
    /// hours answers as the first request did and sends nothing, also across restarts.
    /// </summary>
    public Task<SendEmailResponse> SendEmailAsync(SendEmailRequest request, string idempotencyKey, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrEmpty(idempotencyKey);
        return SendAsync(HttpMethod.Post, "v1/emails", Json(request, PostilioJsonContext.Default.SendEmailRequest),
            PostilioJsonContext.Default.SendEmailResponse, cancellationToken, idempotencyKey);
    }

    /// <summary>Gets a message and its events. Needs the <c>emails:read</c> scope; a key finds only messages of its own mode.</summary>
    public Task<EmailDetails> GetEmailAsync(Guid id, CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Get, $"v1/emails/{id}", null, PostilioJsonContext.Default.EmailDetails, cancellationToken);

    /// <summary>
    /// Cancels a message sent with <see cref="SendEmailRequest.SendAt"/> while it waits: it becomes <c>canceled</c> and
    /// its content is deleted. Needs the <c>emails:send</c> scope. Once it is on its way, or for a message that was never
    /// scheduled, it throws a <see cref="PostilioConflictException"/> (<c>email_not_scheduled</c>).
    /// </summary>
    public Task CancelEmailAsync(Guid id, CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Delete, $"v1/emails/{id}", null, cancellationToken);

    /// <summary>
    /// Sends one test email to an address of your own: one confirmed for test emails in the project, or a member's.
    /// Needs the <c>emails:send</c> scope. A project sends a few a day (429 <c>test_mail_daily_limit_reached</c>);
    /// otherwise it is a send like any other, and counts towards your usage. It takes no Idempotency-Key, so the client
    /// does not retry it after a connection failure or a 5xx.
    /// </summary>
    public Task<TestEmailResponse> SendTestEmailAsync(TestEmailRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return SendAsync(HttpMethod.Post, "v1/emails/test", Json(request, PostilioJsonContext.Default.TestEmailRequest),
            PostilioJsonContext.Default.TestEmailResponse, cancellationToken);
    }

    /// <summary>Gets the project's usage in a UTC month. Needs a live key with the <c>usage:read</c> scope.</summary>
    /// <param name="month"><c>yyyy-MM</c>; the current month when null.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<ApiUsage> GetUsageAsync(string? month = null, CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Get, "v1/usage" + Query(("month", month)), null, PostilioJsonContext.Default.ApiUsage, cancellationToken);

    /// <summary>Adds a sending domain; the answer lists the DNS records to create. Needs <c>domains:manage</c>.</summary>
    public Task<DomainResponse> CreateDomainAsync(CreateDomainRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return SendAsync(HttpMethod.Post, "v1/domains", Json(request, PostilioJsonContext.Default.CreateDomainRequest),
            PostilioJsonContext.Default.DomainResponse, cancellationToken);
    }

    /// <summary>Lists the project's sending domains. Needs <c>domains:manage</c>.</summary>
    public Task<DomainList> ListDomainsAsync(CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Get, "v1/domains", null, PostilioJsonContext.Default.DomainList, cancellationToken);

    /// <summary>Gets a sending domain. Needs <c>domains:manage</c>.</summary>
    public Task<DomainResponse> GetDomainAsync(Guid id, CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Get, $"v1/domains/{id}", null, PostilioJsonContext.Default.DomainResponse, cancellationToken);

    /// <summary>Removes a sending domain. Needs <c>domains:manage</c>.</summary>
    public Task DeleteDomainAsync(Guid id, CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Delete, $"v1/domains/{id}", null, cancellationToken);

    /// <summary>Checks a domain's DNS records now, once a minute at most. Needs <c>domains:manage</c>.</summary>
    public Task<DomainResponse> CheckDomainAsync(Guid id, CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Post, $"v1/domains/{id}/check", null, PostilioJsonContext.Default.DomainResponse, cancellationToken);

    /// <summary>
    /// Lists suppressed addresses, newest first. Needs <c>suppressions:manage</c>. For the next page pass the answer's
    /// <see cref="SuppressionList.Next"/> as <paramref name="before"/>.
    /// </summary>
    /// <param name="q">Part of an address.</param>
    /// <param name="reason">See <see cref="SuppressionReasons"/>.</param>
    /// <param name="before">The <see cref="SuppressionList.Next"/> of the previous page.</param>
    /// <param name="limit">1 to 100; 50 by default.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<SuppressionList> ListSuppressionsAsync(string? q = null, string? reason = null, Guid? before = null, int? limit = null, CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Get, "v1/suppressions" + Query(("q", q), ("reason", reason), ("before", before?.ToString()), ("limit", limit?.ToString(CultureInfo.InvariantCulture))),
            null, PostilioJsonContext.Default.SuppressionList, cancellationToken);

    /// <summary>Puts an address on the suppression list, with the reason <c>manual</c>. Needs <c>suppressions:manage</c>.</summary>
    public Task<SuppressionResponse> CreateSuppressionAsync(CreateSuppressionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return SendAsync(HttpMethod.Post, "v1/suppressions", Json(request, PostilioJsonContext.Default.CreateSuppressionRequest),
            PostilioJsonContext.Default.SuppressionResponse, cancellationToken);
    }

    /// <summary>
    /// Takes an address off the suppression list. Needs <c>suppressions:manage</c>. A complaint only comes off with a
    /// reason of 10 to 500 characters.
    /// </summary>
    public Task DeleteSuppressionAsync(Guid id, RemoveSuppressionRequest? request = null, CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Delete, $"v1/suppressions/{id}", request is null ? null : Json(request, PostilioJsonContext.Default.RemoveSuppressionRequest), cancellationToken);

    /// <summary>Lists the project's webhook endpoints. Needs <c>webhooks:manage</c>.</summary>
    public Task<WebhookEndpointList> ListWebhookEndpointsAsync(CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Get, "v1/webhooks", null, PostilioJsonContext.Default.WebhookEndpointList, cancellationToken);

    /// <summary>Adds a webhook endpoint. The answer holds its signing secret, shown this once. Needs <c>webhooks:manage</c>.</summary>
    public Task<CreatedWebhookEndpoint> CreateWebhookEndpointAsync(CreateWebhookEndpointRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return SendAsync(HttpMethod.Post, "v1/webhooks", Json(request, PostilioJsonContext.Default.CreateWebhookEndpointRequest),
            PostilioJsonContext.Default.CreatedWebhookEndpoint, cancellationToken);
    }

    /// <summary>Gets a webhook endpoint. Needs <c>webhooks:manage</c>.</summary>
    public Task<WebhookEndpointResponse> GetWebhookEndpointAsync(Guid id, CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Get, $"v1/webhooks/{id}", null, PostilioJsonContext.Default.WebhookEndpointResponse, cancellationToken);

    /// <summary>Changes a webhook endpoint; what the request leaves null stays as it is. Needs <c>webhooks:manage</c>.</summary>
    public Task<WebhookEndpointResponse> UpdateWebhookEndpointAsync(Guid id, UpdateWebhookEndpointRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return SendAsync(HttpMethod.Patch, $"v1/webhooks/{id}", Json(request, PostilioJsonContext.Default.UpdateWebhookEndpointRequest),
            PostilioJsonContext.Default.WebhookEndpointResponse, cancellationToken);
    }

    /// <summary>Removes a webhook endpoint. Needs <c>webhooks:manage</c>.</summary>
    public Task DeleteWebhookEndpointAsync(Guid id, CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Delete, $"v1/webhooks/{id}", null, cancellationToken);

    /// <summary>Rotates the signing secret; the old one keeps signing for 24 hours. Needs <c>webhooks:manage</c>.</summary>
    public Task<RotatedWebhookSecret> RotateWebhookSecretAsync(Guid id, CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Post, $"v1/webhooks/{id}/secret", null, PostilioJsonContext.Default.RotatedWebhookSecret, cancellationToken);

    /// <summary>Sends a <c>webhook.test.v1</c> event once, also to a paused endpoint. Needs <c>webhooks:manage</c>.</summary>
    public Task<WebhookDeliveryResponse> SendWebhookTestEventAsync(Guid id, CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Post, $"v1/webhooks/{id}/test", null, PostilioJsonContext.Default.WebhookDeliveryResponse, cancellationToken);

    /// <summary>Lists an endpoint's deliveries, newest first. Needs <c>webhooks:manage</c>.</summary>
    /// <param name="id">The endpoint.</param>
    /// <param name="status">See <see cref="WebhookDeliveryStatuses"/>.</param>
    /// <param name="before">The <see cref="WebhookDeliveryList.Next"/> of the previous page.</param>
    /// <param name="limit">1 to 100; 50 by default.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<WebhookDeliveryList> ListWebhookDeliveriesAsync(Guid id, string? status = null, Guid? before = null, int? limit = null, CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Get, $"v1/webhooks/{id}/deliveries" + Query(("status", status), ("before", before?.ToString()), ("limit", limit?.ToString(CultureInfo.InvariantCulture))),
            null, PostilioJsonContext.Default.WebhookDeliveryList, cancellationToken);

    /// <summary>Tries a delivery once more right away, with the same id and payload. Needs <c>webhooks:manage</c>.</summary>
    public Task RetryWebhookDeliveryAsync(Guid id, Guid deliveryId, CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Post, $"v1/webhooks/{id}/deliveries/{deliveryId}/retry", null, cancellationToken);

    private async Task<T> SendAsync<T>(HttpMethod method, string path, HttpContent? content, JsonTypeInfo<T> answer, CancellationToken cancellationToken, string? idempotencyKey = null)
    {
        using var response = await SendAsync(method, path, content, idempotencyKey, cancellationToken).ConfigureAwait(false);
        try
        {
            return await response.Content.ReadFromJsonAsync(answer, cancellationToken).ConfigureAwait(false)
                ?? throw new JsonException("The body is null.");
        }
        catch (JsonException e)
        {
            throw new PostilioException($"{method} /{path.Split('?')[0]} answered {(int)response.StatusCode} with a body that is not the expected JSON.",
                response.StatusCode, innerException: e);
        }
    }

    private async Task SendAsync(HttpMethod method, string path, HttpContent? content, CancellationToken cancellationToken)
    {
        using var response = await SendAsync(method, path, content, null, cancellationToken).ConfigureAwait(false);
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, HttpContent? content, string? idempotencyKey, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, new Uri(_baseAddress, path)) { Content = content };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        request.Headers.UserAgent.Add(UserAgent);
        if (idempotencyKey is not null)
        {
            request.Headers.Add(RetryHandler.IdempotencyKeyHeader, idempotencyKey);
        }
        var response = await _http.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (response.IsSuccessStatusCode)
        {
            return response;
        }
        using (response)
        {
            throw await ErrorAsync(method, path, response, cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task<PostilioException> ErrorAsync(HttpMethod method, string path, HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var error = TryRead(body, PostilioJsonContext.Default.ErrorResponse);
        var code = error?.Error;
        var problem = TryRead(body, PostilioJsonContext.Default.HttpValidationProblemDetails);
        var status = response.StatusCode;
        var retryAfter = RetryHandler.RetryAfter(response);
        var message = $"{method} /{path.Split('?')[0]} answered {(int)status}{(code is null ? string.Empty : $" ({code})")}"
            + (error?.Message is { } detail ? $": {detail}" : ".");
        return status switch
        {
            HttpStatusCode.BadRequest => new PostilioValidationException(message, problem?.Errors ?? [], problem?.TraceId),
            HttpStatusCode.Unauthorized => new PostilioAuthenticationException(message, status, code),
            HttpStatusCode.Forbidden => new PostilioPermissionException(message, status, code),
            HttpStatusCode.NotFound => new PostilioNotFoundException(message, status),
            HttpStatusCode.Conflict => new PostilioConflictException(message, status, code),
            HttpStatusCode.UnprocessableEntity => new PostilioUnprocessableException(message, status, code),
            HttpStatusCode.TooManyRequests => new PostilioRateLimitException(message, status, code, retryAfter: retryAfter),
            >= HttpStatusCode.InternalServerError => new PostilioServerException(message, status, code, problem?.TraceId, retryAfter),
            _ => new PostilioException(message, status, code, problem?.TraceId, retryAfter),
        };
    }

    private static T? TryRead<T>(string body, JsonTypeInfo<T> type)
    {
        try
        {
            return body.Length == 0 ? default : JsonSerializer.Deserialize(body, type);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    private static JsonContent Json<T>(T value, JsonTypeInfo<T> type) => JsonContent.Create(value, type);

    private static string Query(params (string Name, string? Value)[] parameters)
    {
        var present = new List<string>();
        foreach (var (name, value) in parameters)
        {
            if (value is not null)
            {
                present.Add($"{name}={Uri.EscapeDataString(value)}");
            }
        }
        return present.Count == 0 ? string.Empty : "?" + string.Join('&', present);
    }

}
