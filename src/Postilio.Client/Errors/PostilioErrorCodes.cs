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

    /// <summary>403: Postilio suspended your organization; no project sends until it is resumed.</summary>
    public const string OrganizationSuspended = "organization_suspended";

    /// <summary>403: the key's project is paused or suspended; your other projects keep sending.</summary>
    public const string ProjectSuspended = "project_suspended";

    /// <summary>409: the Idempotency-Key was used for another request in the last 24 hours.</summary>
    public const string IdempotencyKeyReusedWithDifferentRequest = "idempotency_key_reused_with_different_request";

    /// <summary>409: the project has this domain already.</summary>
    public const string DomainExists = "domain_exists";

    /// <summary>409: your plan allows no more domains.</summary>
    public const string DomainLimitReached = "domain_limit_reached";

    /// <summary>409: the address is on the suppression list already.</summary>
    public const string AddressAlreadySuppressed = "address_already_suppressed";

    /// <summary>409: the project has 10 webhook endpoints.</summary>
    public const string WebhookEndpointLimitReached = "webhook_endpoint_limit_reached";

    /// <summary>409: the delivery is still being retried.</summary>
    public const string DeliveryStillPending = "delivery_still_pending";

    /// <summary>409: resume the webhook endpoint first.</summary>
    public const string EndpointPaused = "endpoint_paused";

    /// <summary>409: the message is not waiting for its send time (any more): it was not scheduled, was canceled, or is on its way.</summary>
    public const string EmailNotScheduled = "email_not_scheduled";

    /// <summary>409: the project holds 10,000 scheduled messages.</summary>
    public const string ScheduledLimitReached = "scheduled_limit_reached";

    /// <summary>409: the project's scheduled messages would hold more than 256 MB together.</summary>
    public const string ScheduledSizeLimitReached = "scheduled_size_limit_reached";

    /// <summary>413: the message is over 10 MB, attachments included.</summary>
    public const string MessageTooLarge = "message_too_large";

    /// <summary>413: the message's size times its recipients is over 25 MB; send to fewer recipients at a time.</summary>
    public const string MessageTooLargeForRecipients = "message_too_large_for_recipients";

    /// <summary>413: the request body is over what the endpoint reads (21 MB for sending, 1 MB elsewhere).</summary>
    public const string PayloadTooLarge = "payload_too_large";

    /// <summary>422: the sender's domain is not verified, or has been failing for over 72 hours.</summary>
    public const string UnverifiedSenderDomain = "unverified_sender_domain";

    /// <summary>422: the key is restricted to other sending domains.</summary>
    public const string SenderDomainNotAllowedForKey = "sender_domain_not_allowed_for_key";

    /// <summary>422: in the sandbox, a recipient outside your team and verified domains.</summary>
    public const string SandboxRecipientNotAllowed = "sandbox_recipient_not_allowed";

    /// <summary>422: the send time is less than a minute ahead, or in the past.</summary>
    public const string SendAtTooSoon = "send_at_too_soon";

    /// <summary>422: the send time is more than 30 days ahead.</summary>
    public const string SendAtTooFar = "send_at_too_far";

    /// <summary>422: cc or bcc with more than one address in to.</summary>
    public const string CcBccRequireSingleTo = "cc_bcc_require_single_to";

    /// <summary>422: a test email to an address that is neither confirmed for test emails nor a member's.</summary>
    public const string TestRecipientNotConfirmed = "test_recipient_not_confirmed";

    /// <summary>422: removing a complaint from the suppression list needs a reason.</summary>
    public const string ReasonRequired = "reason_required";

    /// <summary>429: in the sandbox, the day's recipients are used up.</summary>
    public const string SandboxDailyLimitReached = "sandbox_daily_limit_reached";

    /// <summary>429: in the sandbox, this minute's recipients are used up.</summary>
    public const string SandboxRateLimitReached = "sandbox_rate_limit_reached";

    /// <summary>429: the project's own limit: the day's recipients are used up.</summary>
    public const string ProjectDailyLimitReached = "project_daily_limit_reached";

    /// <summary>429: the project's own limit: this minute's recipients are used up.</summary>
    public const string ProjectRateLimitReached = "project_rate_limit_reached";

    /// <summary>429: your plan's month is used up.</summary>
    public const string PlanMonthlyLimitReached = "plan_monthly_limit_reached";

    /// <summary>429: your plan's day is used up.</summary>
    public const string PlanDailyLimitReached = "plan_daily_limit_reached";

    /// <summary>429: your plan's minute is used up.</summary>
    public const string PlanRateLimitReached = "plan_rate_limit_reached";

    /// <summary>429: the platform's safety limit per key or project: this minute's recipients are used up.</summary>
    public const string PlatformRateLimitReached = "platform_rate_limit_reached";

    /// <summary>429: the platform's safety limit per project: the day's recipients are used up.</summary>
    public const string PlatformDailyLimitReached = "platform_daily_limit_reached";

    /// <summary>429: the project's test emails for the day (UTC) are used up.</summary>
    public const string TestMailDailyLimitReached = "test_mail_daily_limit_reached";

    /// <summary>429: a domain can be checked once a minute.</summary>
    public const string TooManyChecks = "too_many_checks";

    /// <summary>503: the domain check got no answer from DNS.</summary>
    public const string DnsUnavailable = "dns_unavailable";

    /// <summary>503: Postilio takes no new messages for now, to protect its storage; nothing was stored, so send it again after the Retry-After.</summary>
    public const string ServiceDegraded = "service_degraded";
}
