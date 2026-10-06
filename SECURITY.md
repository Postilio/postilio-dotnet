# Security

## Reporting a vulnerability

Please do not open a public issue. Report it privately through GitHub's **Report a vulnerability** button on this
repository's **Security** tab. We answer within five working days and keep you informed until it is fixed. Include the
version, what an attacker can do, and the steps or code to reproduce it.

A vulnerability in the Postilio service itself (the API, the portal, SMTP) is reported the same way; we pass it on.

## Supported versions

Only the latest release gets security fixes while the SDK is below 1.0.

## How the SDK handles secrets

- The API key is sent only in the `Authorization` header, to the configured `BaseAddress`. The client never logs it,
  `AddPostilio` redacts the header from `IHttpClientFactory`'s logs, and neither exceptions nor `PostilioOptions.ToString()`
  contain it. Keep the key in a secret store, not in `appsettings.json` or source control.
- Webhook secrets (`whsec_…`) are only read by `WebhookVerifier`, which compares signatures in constant time and refuses a
  timestamp more than five minutes off. Always verify a delivery before acting on it, and verify the raw body.
- The repository holds no keys. The contract tests read a test key from environment variables.
