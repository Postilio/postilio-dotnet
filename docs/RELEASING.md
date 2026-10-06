# Releasing

Nothing is published yet. This is the process, and what has to be in place before the first release.

## Once, before the first release

1. **A license.** Choose one (MIT or Apache-2.0 are the usual choices for an SDK; Apache-2.0 adds an explicit patent
   grant), add `LICENSE`, set `<PackageLicenseExpression>` in `src/Directory.Build.props`, and update the README.
2. **A nuget.org organization** `Postilio`, owned by a personal nuget.org account with two-factor authentication.
   Packages are pushed under the organization, so ownership does not hang on one person.
3. **Reserve the ID prefix** `Postilio.*` (and the bare `Postilio`), by mail to `account@nuget.org` from the
   organization's owner, with proof of the domain (`postilio.eu`). A reserved prefix gets the verified checkmark and
   stops others from publishing `Postilio.Something`. Until the reservation is through, push the first version
   early, so the IDs `Postilio.Client` and `Postilio.Client.AspNetCore` are taken by us.
4. **Publishing credentials.** Either:
   - **Trusted publishing** (recommended once CI runs): nuget.org trusts a GitHub Actions workflow of this repository
     through OIDC and hands it a short-lived key per run. No long-lived secret anywhere. Needs CI; see
     [CI.md](CI.md).
   - **An API key** for pushing by hand: scoped to *Push new packages and package versions*, glob `Postilio.*`,
     expiring after 365 days at most. Keep it in a password manager, never in the repository or shell history.
5. **Signing.** nuget.org repository-signs every package, which is enough for most consumers. Author signing
   (`dotnet nuget sign`) needs a code-signing certificate from a CA, for a yearly fee, and is optional; it
   shows consumers the package came from Postilio and not only through nuget.org. Commits and tags are signed already.
6. **GitHub:** turn on *Private vulnerability reporting* (Settings → Security), which `SECURITY.md` refers to, and
   protect `main` (pull requests only, signed commits).

## Every release

1. On `main`, move the *Unreleased* entries in `CHANGELOG.md` to a new version heading with the date.
2. Set `<Version>` in `src/Directory.Build.props`. Semantic versioning: a removed or changed public member is a major
   version (a minor one while below 1.0); a new member a minor; a fix a patch. Pre-releases: `0.2.0-alpha.1`.
3. Run `./build.sh` (with the contract tests against a test environment: see `CONTRIBUTING.md`). It builds, tests, packs
   to `artifacts/packages/<version>/` and checks the packages.
4. Commit (`chore: release 0.2.0`), and tag it signed: `git tag -s v0.2.0 -m "v0.2.0"`. Push the commit and the tag.
5. Push the packages; the symbol packages (`.snupkg`) go along by themselves:

   ```sh
   dotnet nuget push "artifacts/packages/0.2.0/*.nupkg" --source https://api.nuget.org/v3/index.json --api-key <key>
   ```

6. Create a GitHub release from the tag, with the changelog section as its notes.
7. After the first release: set `<PackageValidationBaselineVersion>` in `src/Directory.Build.props` to the released
   version, so `dotnet pack` fails on an accidental breaking change from then on.

## Support window

The packages target .NET 8 (LTS, supported by Microsoft until 10 November 2026) and .NET 10 (LTS). Drop .NET 8 in the
first minor release after its end of support, and add the next LTS when it ships.
