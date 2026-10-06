using Microsoft.Extensions.Options;

namespace Postilio.DependencyInjection;

internal sealed class PostilioOptionsValidator : IValidateOptions<PostilioOptions>
{
    public ValidateOptionsResult Validate(string? name, PostilioOptions options) =>
        options.Problem() is { } problem ? ValidateOptionsResult.Fail(problem) : ValidateOptionsResult.Success;
}
