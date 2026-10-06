using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Postilio.DependencyInjection;

// Lets IOptionsMonitor pick up a reloaded section, such as a rotated key, without the Options.ConfigurationExtensions package.
internal sealed class ConfigurationChangeTokenSource(IConfiguration configuration) : IOptionsChangeTokenSource<PostilioOptions>
{
    public string Name => Options.DefaultName;

    public IChangeToken GetChangeToken() => configuration.GetReloadToken();
}
