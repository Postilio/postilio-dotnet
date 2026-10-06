# Contributing

Thank you for helping. Open an issue before a large change, so we can agree on the approach first.

## Build and test

You need the .NET 10 SDK and the .NET 8 runtime (the tests run on both targets).

```sh
./build.sh
```

It builds with warnings as errors, runs the tests, packs both packages into `artifacts/packages/<version>/` and checks
their contents (`tools/check-packages.py`: files and dependencies per target, docs, symbols, no secrets). Run it before
you open a pull request.

## Conventions

- Code, comments, docs, commits and pull requests in English.
- Commits: `<type>(<scope>): <subject>`, imperative and lowercase (`feat`, `fix`, `docs`, `test`, `refactor`, `chore`).
  Signed commits are welcome.
- One public type per file; models are named exactly as the schemas in the OpenAPI document.
- Every public member has XML docs; every behaviour has one test (`Method_Scenario_Expected`).
- No new dependencies without discussing them first. No reflection-based JSON: add new types to
  `PostilioJsonContext`, so the packages stay trimming- and AOT-compatible.
- Add a line to `CHANGELOG.md` under *Unreleased*.

## The OpenAPI document

The client is written by hand, and held to the API by tests rather than generated from it:

- `spec/openapi-v1.json` is a copy of the API's `/v1` document (`openapi/Postilio.Api.json` in the Postilio platform
  repository, also served at `/openapi/v1.json`). `spec/openapi-v1.sha256` is its fingerprint, the same value the
  platform keeps in `docs/openapi.sha256`, so comparing the two tells whether the copy is current.
- `SpecTests` fail when the client and the copy disagree: a method per `operationId`, and a type per schema with the
  same fields, types and nullability.
- `ContractTests` run against a running Postilio; one of them compares the server's document with the copy.

When the API changes:

1. Copy the new document over `spec/openapi-v1.json` and write its SHA-256 to `spec/openapi-v1.sha256`
   (`sha256sum spec/openapi-v1.json | cut -d' ' -f1 > spec/openapi-v1.sha256`).
2. Run the tests; `SpecTests` name every difference. Follow them in the models and `PostilioClient`.
3. Read the changed guides (errors, idempotency, webhooks) for behaviour the document does not show, such as a new
   error code (`PostilioErrorCodes`) or retry rule.
4. Add the change to `CHANGELOG.md`; a removed or renamed member is a breaking change.

## Contract tests

They are skipped unless these environment variables are set:

| Variable | Value |
|---|---|
| `POSTILIO_CONTRACT_BASE_ADDRESS` | The API, such as `http://localhost:26299` for a local environment |
| `POSTILIO_CONTRACT_API_KEY` | A **test** key (`pk_test_…`) with `emails:send` and `emails:read`; nothing is delivered |
| `POSTILIO_CONTRACT_FROM` | An address on a verified domain of the key's project |

Create the key in the portal under **Keys & SMTP** and keep it out of the repository, in your shell or a file outside
it. Run only the contract tests with:

```sh
dotnet test --solution Postilio.slnx -- --filter-trait "Category=Contract"
```
