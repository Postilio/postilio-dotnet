using Microsoft.AspNetCore.Identity;
using Postilio.AspNetCore;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Registers <see cref="PostilioEmailSender{TUser}"/> as Identity's email sender.</summary>
public static class PostilioEmailSenderServiceCollectionExtensions
{
    /// <summary>
    /// Sends Identity's emails (confirmation links, password resets) through Postilio, as both <see cref="IEmailSender{TUser}"/>
    /// and the Identity UI's <c>IEmailSender</c>. Register the client first with <c>services.AddPostilio(...)</c>; the
    /// sender address is checked when the app starts.
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
        services.AddTransient<PostilioEmailSender<TUser>>();
        services.AddTransient<IEmailSender<TUser>>(provider => provider.GetRequiredService<PostilioEmailSender<TUser>>());
        services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender>(provider => provider.GetRequiredService<PostilioEmailSender<TUser>>());
        return services;
    }
}
