# Releasing

A release is a pushed tag `v<version>`: [`release.yml`](../.github/workflows/release.yml) builds, tests and packs, pushes
both packages to nuget.org through trusted publishing, and creates the GitHub release. Nothing is published yet.

## Once, before the first release

1. **A nuget.org account** for the owner: sign in on nuget.org with a Microsoft account, with two-factor authentication.
2. **The organization `Postilio`**: your name (top right) → *Manage Organizations…* → *Add new organization*. Its e-mail
   address must belong to no other nuget.org account (for example `nuget@postilio.eu`).
3. **A trusted publishing policy** at [nuget.org/account/trustedpublishing](https://www.nuget.org/account/trustedpublishing):
   package owner `Postilio` (the organization), repository owner `Postilio`, repository `postilio-dotnet`, workflow file
   `release.yml`, allowed to push new packages and new versions. No API key is stored anywhere: per run, nuget.org hands
   the workflow a key valid for one hour.
4. **The repository secret `NUGET_USER`**: your personal nuget.org user name (the profile name, not the e-mail address and
   not the organization). It names who logs in; it is not a secret in itself, but kept out of the workflow file.
5. **Reserve the ID prefix** `Postilio.*` (optional, recommended): mail `account@nuget.org`, preferably from an
   @postilio.eu address, naming the organization `Postilio` and the prefix, with links to postilio.eu and
   github.com/Postilio. They look for consistent package metadata, including a license expression (set) and an **icon**
   (not yet: add `PackageIcon` once there is one). The first release does not have to wait for it.

## Every release

1. On `main`, move the *Unreleased* entries in `CHANGELOG.md` to a new version heading with the date.
2. Set `<Version>` in `src/Directory.Build.props`. Semantic versioning: a removed or changed public member is a major
   version (a minor one while below 1.0); a new member a minor; a fix a patch. Pre-releases: `0.2.0-alpha.1`.
3. Run `./build.sh` locally, with the contract tests against a test environment (see `CONTRIBUTING.md`); CI cannot run
   those.
4. Commit (`chore: release 0.2.0`) through a pull request, then tag the merged commit signed and push the tag:

   ```sh
   git tag -s v0.2.0 -m "v0.2.0"
   git push origin v0.2.0
   ```

5. `release.yml` checks that the tag matches `<Version>`, runs `./build.sh`, pushes the packages (symbols included) and
   creates the GitHub release, marked as a pre-release for a version with a suffix. A release made by hand in GitHub
   for the tag works too: the workflow then adds the packages to it. Check the package pages afterwards:
   the owner must be the organization `Postilio`.
6. After the first release: set `<PackageValidationBaselineVersion>` in `src/Directory.Build.props` to the released
   version, so `dotnet pack` fails on an accidental breaking change from then on.

A failed run can be run again from the Actions tab: packages already on nuget.org are skipped, and a version is never
published twice. New packages take a while (validation and indexing) before their page on nuget.org shows.

## Signing

nuget.org repository-signs every package. Author signing (`dotnet nuget sign`) needs a code-signing certificate from a
CA, for a yearly fee; it is optional. Commits and tags are signed already.

## Support window

The packages target .NET 8 (LTS, supported by Microsoft until 10 November 2026) and .NET 10 (LTS). Drop .NET 8 in the
first minor release after its end of support, and add the next LTS when it ships.
