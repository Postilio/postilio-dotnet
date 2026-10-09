namespace Postilio;

/// <summary>
/// Why a delivery attempt was delayed or failed, or why a scheduled message was canceled
/// (<see cref="EmailEvent.Reason"/>). New ones may be added: treat an unknown one on a <c>deferred</c> event as
/// <see cref="UnknownTemporary"/> and on a <c>bounced</c> one as <see cref="UnknownPermanent"/>.
/// </summary>
public static class EmailEventReasons
{
    /// <summary>The receiving server refused for now (a 4xx reply), greylisting included; Postilio retries.</summary>
    public const string RecipientServerTemporary = "recipient_server_temporary";

    /// <summary>The receiving server asks Postilio to slow down; Postilio retries later.</summary>
    public const string RateLimitedByRecipient = "rate_limited_by_recipient";

    /// <summary>The recipient's mailbox is full (temporary or permanent, see the SMTP code).</summary>
    public const string MailboxFull = "mailbox_full";

    /// <summary>The receiving server refused the message for good (a 5xx reply).</summary>
    public const string RecipientRejected = "recipient_rejected";

    /// <summary>No secure connection: the receiving server's certificate is not valid for its name.</summary>
    public const string TlsCertificateInvalid = "tls_certificate_invalid";

    /// <summary>The secure connection to the receiving server failed.</summary>
    public const string TlsFailed = "tls_failed";

    /// <summary>The receiving server did not respond in time.</summary>
    public const string ConnectionTimeout = "connection_timeout";

    /// <summary>No connection to the receiving server could be made.</summary>
    public const string ConnectionFailed = "connection_failed";

    /// <summary>A connection to one of the recipient's mail servers failed.</summary>
    public const string Ipv6CandidateFailed = "ipv6_candidate_failed";

    /// <summary>No reachable mail server was found for the recipient's domain.</summary>
    public const string DnsOrMxFailed = "dns_or_mx_failed";

    /// <summary>Postilio paces delivery to this mailbox provider.</summary>
    public const string DeliveryPaced = "delivery_paced";

    /// <summary>Delivery is paused on Postilio's side for the moment.</summary>
    public const string DeliveryPaused = "delivery_paused";

    /// <summary>Not delivered within its retry period; the message is expired.</summary>
    public const string RetryPeriodEnded = "retry_period_ended";

    /// <summary>A temporary problem without a more specific reason.</summary>
    public const string UnknownTemporary = "unknown_temporary";

    /// <summary>A permanent problem without a more specific reason.</summary>
    public const string UnknownPermanent = "unknown_permanent";

    /// <summary>
    /// The receiving server accepted the message and later sent a bounce report: a <c>bounced</c> event after the
    /// <c>delivered</c> one, with <see cref="EmailEvent.Async"/> true.
    /// </summary>
    public const string AsyncBounce = "async_bounce";

    /// <summary>A scheduled message you canceled.</summary>
    public const string CanceledByRequest = "canceled_by_request";

    /// <summary>A scheduled message lost when Postilio restored its database from a backup, which holds no content: send it again.</summary>
    public const string LostInRestore = "lost_in_restore";
}
