using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Postilio.AspNetCore;

/// <summary>
/// Sends ASP.NET Core Identity's emails through Postilio: as <see cref="IEmailSender{TUser}"/> with Identity's own
/// wording, as HTML and plain text, and as the Identity UI's <see cref="Microsoft.AspNetCore.Identity.UI.Services.IEmailSender"/>.
/// Register it with <c>services.AddPostilioEmailSender&lt;TUser&gt;(...)</c>.
/// </summary>
/// <typeparam name="TUser">The app's user type.</typeparam>
public sealed class PostilioEmailSender<TUser>(PostilioClient client, IOptions<PostilioEmailSenderOptions> options)
    : IEmailSender<TUser>, Microsoft.AspNetCore.Identity.UI.Services.IEmailSender
    where TUser : class
{
    // Read here, so a missing sender address fails when the sender is resolved rather than at the first email.
    private readonly PostilioEmailSenderOptions _options = options.Value;

    /// <inheritdoc />
    public Task SendConfirmationLinkAsync(TUser user, string email, string confirmationLink) =>
        SendAsync(email, "Confirm your email",
            $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.",
            $"Please confirm your account by opening this link: {WebUtility.HtmlDecode(confirmationLink)}");

    /// <inheritdoc />
    public Task SendPasswordResetLinkAsync(TUser user, string email, string resetLink) =>
        SendAsync(email, "Reset your password",
            $"Please reset your password by <a href='{resetLink}'>clicking here</a>.",
            $"Please reset your password by opening this link: {WebUtility.HtmlDecode(resetLink)}");

    /// <inheritdoc />
    public Task SendPasswordResetCodeAsync(TUser user, string email, string resetCode) =>
        SendAsync(email, "Reset your password",
            $"Please reset your password using the following code: {WebUtility.HtmlEncode(resetCode)}",
            $"Please reset your password using the following code: {resetCode}");

    /// <inheritdoc />
    public Task SendEmailAsync(string email, string subject, string htmlMessage) => SendAsync(email, subject, htmlMessage, null);

    // Identity passes links already HTML-encoded, so they go into the HTML as they are and are decoded for the text.
    private Task<SendEmailResponse> SendAsync(string email, string subject, string html, string? text) =>
        client.SendEmailAsync(new SendEmailRequest
        {
            From = _options.From,
            To = [email],
            Subject = subject,
            Html = html,
            Text = text,
            Tag = _options.Tag,
        });
}
