# Continuous integration: a proposal

There is no active workflow in this repository on purpose: a workflow uses GitHub Actions minutes, and that is a
decision for the owner. Until then, `./build.sh` is the check to run before every push.

## What decides the cost

- **A public repository** runs on GitHub's standard hosted runners **for free**, without a minutes limit. An SDK is
  normally public, so the workflow below would cost nothing.
- **A private repository** uses the account's included minutes (they reset monthly), then pay-as-you-go. The workflow
  below takes about 3 minutes per run on `ubuntu-latest`.
- **A self-hosted runner** costs no minutes, but never attach one to a public repository: a pull request from a fork
  would run its own code on that machine.

## The workflow

When the repository is public (or the minutes are there), save this as `.github/workflows/build.yml`:

```yaml
name: build
on:
  push:
    branches: [main]
  pull_request:
permissions:
  contents: read
jobs:
  build:
    runs-on: ubuntu-latest
    timeout-minutes: 15
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: |
            8.0.x
            10.0.x
      - run: ./build.sh
        env:
          CI: true
      - uses: actions/upload-artifact@v4
        with:
          name: packages
          path: artifacts/packages
```

The contract tests stay skipped there: they need a running Postilio and a test key. Run them locally before a release,
or later against a staging environment with the key as a repository secret.

## Publishing from CI (later)

With nuget.org trusted publishing, a second workflow on a pushed `v*` tag can pack and push without any stored key:
nuget.org trusts this repository's workflow through OIDC. Set that up together with the first release
([RELEASING.md](RELEASING.md)).
