using Microsoft.AspNetCore.Identity;
using Postilio.AspNetCore;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Registers <see cref="PostilioEmailSender{TUser}"/> as Identity's <see cref="IEmailSender{TUser}"/>.</summary>
public static class PostilioEmailSenderServiceCollectionExtensions
{
    /// <summary>
    /// Sends Identity's emails (confirmation links, password resets) through Postilio. Register the client first with
    /// <c>services.AddPostilio(...)</c>; the sender address is checked when the app starts.
    /// </summary>
    public static IServiceCollection AddPostilioEmailSender<TUser>(this IServiceCollection services, Action<PostilioEmailSenderOptions> configure)
        where TUser : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);
        services.AddOptions<PostilioEmailSenderOptions>()
            .Configure(configure)
            .Validate(o => !string.IsNullOrWhiteSpace(o.From), "PostilioEmailSenderOptions.From is required: an address on a verified domain.")
            .ValidateOnStart();
        services.AddTransient<IEmailSender<TUser>, PostilioEmailSender<TUser>>();
        return services;
    }
}
