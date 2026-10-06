namespace Postilio.AspNetCore;

/// <summary>Settings of <see cref="PostilioEmailSender{TUser}"/>.</summary>
public sealed class PostilioEmailSenderOptions
{
    /// <summary>The sender: an address on a verified domain, optionally with a name: <c>Acme &lt;no-reply@mail.example.com&gt;</c>.</summary>
    public string From { get; set; } = string.Empty;

    /// <summary>The tag of these messages in the portal and in webhook events: <c>identity</c> by default, null for none.</summary>
    public string? Tag { get; set; } = "identity";
}
