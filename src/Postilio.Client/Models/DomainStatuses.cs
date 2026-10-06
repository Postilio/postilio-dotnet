namespace Postilio;

/// <summary>The statuses of a sending domain.</summary>
public static class DomainStatuses
{
    /// <summary>Not all records found yet; nothing can be sent from it.</summary>
    public const string Pending = "pending";

    /// <summary>All records found.</summary>
    public const string Verified = "verified";

    /// <summary>A record went missing; it keeps sending for 72 hours after <see cref="DomainResponse.FailingSince"/>.</summary>
    public const string Failing = "failing";
}
