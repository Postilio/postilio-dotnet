namespace Postilio;

/// <summary>A sending domain to add.</summary>
public sealed class CreateDomainRequest
{
    /// <summary>The domain, such as <c>mail.example.com</c>: at least two labels, not an IP address.</summary>
    public required string Name { get; init; }
}
