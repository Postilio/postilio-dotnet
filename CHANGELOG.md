# Changelog

All notable changes to this project are documented here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and the project uses
[semantic versioning](https://semver.org/).

## [Unreleased]

## [0.1.0-alpha.1]

First version, against the alpha of the Postilio API (`/v1`, OpenAPI fingerprint
`7282530d721eef13bace935ab1a7fb445c3537cf3b0acdd81bb9266ed71bf6e8`).

### Added

- `PostilioClient` with a method per operation of `/v1`: send and get email, domains, suppressions, webhook
  endpoints and deliveries.
- An `Idempotency-Key` on every send, made per call unless you pass your own.
- A `User-Agent` of `postilio-dotnet/<version>` on every request; the API key goes on each request, so an
  `HttpClient` you pass in is left as it is.
- Retries: a 429 for every call within `MaxRetryDelay`, honouring `Retry-After`; a connection failure or a
  408/500/502/503/504 for GETs and sends only.
- An exception per status with the API's stable error code (`PostilioErrorCodes`), the validation problems per field,
  `RetryAfter` and the trace id of a server error.
- `services.AddPostilio(...)`: a typed client of `IHttpClientFactory`, options from code or configuration (reloads
  included), the settings validated on start, the API key never logged.
- `WebhookVerifier` (Standard Webhooks: constant-time comparison, timestamp tolerance, secret rotation) and
  `WebhookEvent.Parse`.
- `Postilio.Client.AspNetCore`: `IEmailSender<TUser>` and the Identity UI's `IEmailSender` for ASP.NET Core Identity.
- Targets .NET 8 and .NET 10; trimming- and native-AOT-compatible (System.Text.Json source generation).
- MIT license.
