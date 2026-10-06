namespace Postilio;

/// <summary>The events a webhook endpoint can get. New ones may be added.</summary>
public static class WebhookEventTypes
{
    /// <summary>The API or SMTP took the message.</summary>
    public const string Accepted = EmailStatuses.Accepted;

    /// <summary>Handed to the mail server.</summary>
    public const string Queued = EmailStatuses.Queued;

    /// <summary>The receiving server accepted it.</summary>
    public const string Delivered = EmailStatuses.Delivered;

    /// <summary>A temporary failure; it is retried.</summary>
    public const string Deferred = EmailStatuses.Deferred;

    /// <summary>Refused for good.</summary>
    public const string Bounced = EmailStatuses.Bounced;

    /// <summary>Not delivered within a day.</summary>
    public const string Expired = EmailStatuses.Expired;

    /// <summary>The recipient marked it as spam.</summary>
    public const string Complained = EmailStatuses.Complained;

    /// <summary>Not sent: the address is on the suppression list.</summary>
    public const string Suppressed = EmailStatuses.Suppressed;
}
