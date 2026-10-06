namespace Postilio;

/// <summary>The stable error codes of the API (<see cref="PostilioException.ErrorCode"/>). New ones may be added.</summary>
public static class PostilioErrorCodes
{
    /// <summary>401: the key is missing, unknown or revoked.</summary>
    public const string InvalidApiKey = "invalid_api_key";

    /// <summary>403: the key lacks the scope of this call.</summary>
    public const string InsufficientScope = "insufficient_scope";

    /// <summary>403: the key may not be used from this address.</summary>
    public const string ClientIpNotAllowed = "client_ip_not_allowed";

    /// <summary>409: the Idempotency-Key was used for another request in the last 24 hours.</summary>
    public const string IdempotencyKeyReusedWithDifferentRequest = "idempotency_key_reused_with_different_request";

    /// <summary>409: the project has this domain already.</summary>
    public const string DomainExists = "domain_exists";

    /// <summary>409: the address is on the suppression list already.</summary>
    public const string AddressAlreadySuppressed = "address_already_suppressed";

    /// <summary>409: the project has 10 webhook endpoints.</summary>
    public const string WebhookEndpointLimitReached = "webhook_endpoint_limit_reached";

    /// <summary>409: the delivery is still being retried.</summary>
    public const string DeliveryStillPending = "delivery_still_pending";

    /// <summary>409: resume the webhook endpoint first.</summary>
    public const string EndpointPaused = "endpoint_paused";

    /// <summary>422: the sender's domain is not verified, or has been failing for over 72 hours.</summary>
    public const string UnverifiedSenderDomain = "unverified_sender_domain";

    /// <summary>422: the key is restricted to other sending domains.</summary>
    public const string SenderDomainNotAllowedForKey = "sender_domain_not_allowed_for_key";

    /// <summary>422: in the sandbox, a recipient outside your team and verified domains.</summary>
    public const string SandboxRecipientNotAllowed = "sandbox_recipient_not_allowed";

    /// <summary>422: removing a complaint from the suppression list needs a reason.</summary>
    public const string ReasonRequired = "reason_required";

    /// <summary>429: in the sandbox, the day's recipients are used up.</summary>
    public const string SandboxDailyLimitReached = "sandbox_daily_limit_reached";

    /// <summary>429: in the sandbox, this minute's recipients are used up.</summary>
    public const string SandboxRateLimitReached = "sandbox_rate_limit_reached";

    /// <summary>429: a domain can be checked once a minute.</summary>
    public const string TooManyChecks = "too_many_checks";

    /// <summary>503: the domain check got no answer from DNS.</summary>
    public const string DnsUnavailable = "dns_unavailable";
}
