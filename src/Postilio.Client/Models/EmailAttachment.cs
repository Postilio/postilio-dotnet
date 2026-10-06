namespace Postilio;

/// <summary>A file attached to an email; one with a <see cref="ContentId"/> is inline.</summary>
public sealed class EmailAttachment
{
    /// <summary>The file name the recipient sees.</summary>
    public required string FileName { get; init; }

    /// <summary>The media type, such as <c>application/pdf</c>.</summary>
    public required string ContentType { get; init; }

    /// <summary>The file's bytes; sent as base64.</summary>
    public required byte[] Content { get; init; }

    /// <summary>For an inline file: the HTML refers to it as <c>cid:</c> plus this id.</summary>
    public string? ContentId { get; init; }
}
