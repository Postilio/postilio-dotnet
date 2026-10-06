using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Postilio;
using Postilio.DependencyInjection;
using Postilio.Http;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Registers <see cref="PostilioClient"/> as a typed client of <c>IHttpClientFactory</c>.</summary>
public static class PostilioServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="PostilioClient"/> with retries; the API key is checked when the app starts and is redacted
    /// from the factory's logs. Returns the client's builder, to add handlers or a timeout.
    /// </summary>
    public static IHttpClientBuilder AddPostilio(this IServiceCollection services, Action<PostilioOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);
        services.AddOptions<PostilioOptions>().Configure(configure).ValidateOnStart();
        var builder = services.AddHttpClient(nameof(PostilioClient));
        // A second call only adds settings: a second retry handler would multiply the attempts.
        if (services.Any(s => s.ServiceType == typeof(IValidateOptions<PostilioOptions>)))
        {
            return builder;
        }
        services.AddSingleton<IValidateOptions<PostilioOptions>, PostilioOptionsValidator>();
        return builder
            .AddTypedClient((http, provider) => new PostilioClient(http, provider.GetRequiredService<IOptionsMonitor<PostilioOptions>>().CurrentValue))
            .AddHttpMessageHandler(provider =>
            {
                var options = provider.GetRequiredService<IOptionsMonitor<PostilioOptions>>();
                return new RetryHandler(() => options.CurrentValue, (wait, ct) => Task.Delay(wait, ct));
            })
            // Recycled connections, so a client held by a singleton still follows DNS changes.
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(2) })
            .RedactLoggedHeaders(["Authorization"]);
    }

    /// <summary>
    /// Registers <see cref="PostilioClient"/> with the settings of a configuration section, such as
    /// <c>builder.Configuration.GetSection("Postilio")</c>: <c>ApiKey</c>, <c>BaseAddress</c>, <c>MaxRetries</c>, <c>MaxRetryDelay</c>.
    /// </summary>
    public static IHttpClientBuilder AddPostilio(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        services.AddSingleton<IOptionsChangeTokenSource<PostilioOptions>>(new ConfigurationChangeTokenSource(configuration));
        // Read by hand rather than with the configuration binder, which needs reflection.
        return services.AddPostilio(o =>
        {
            if (configuration[nameof(PostilioOptions.ApiKey)] is { } apiKey)
            {
                o.ApiKey = apiKey;
            }
            if (configuration[nameof(PostilioOptions.BaseAddress)] is { } baseAddress)
            {
                o.BaseAddress = new Uri(baseAddress, UriKind.Absolute);
            }
            if (configuration[nameof(PostilioOptions.MaxRetries)] is { } maxRetries)
            {
                o.MaxRetries = int.Parse(maxRetries, CultureInfo.InvariantCulture);
            }
            if (configuration[nameof(PostilioOptions.MaxRetryDelay)] is { } maxRetryDelay)
            {
                o.MaxRetryDelay = TimeSpan.Parse(maxRetryDelay, CultureInfo.InvariantCulture);
            }
        });
    }
}
