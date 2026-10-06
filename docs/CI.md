# Continuous integration

Two workflows, both on GitHub's standard hosted runners. The repository is public, so they cost no Actions minutes.

| Workflow | When | What |
|---|---|---|
| [`build.yml`](../.github/workflows/build.yml) | every push to `main` and every pull request | `./build.sh`: build with warnings as errors, tests on net8.0 and net10.0, pack, package check; the packages as an artifact |
| [`release.yml`](../.github/workflows/release.yml) | a pushed tag `v*` | the same, then publishes to nuget.org and creates the GitHub release (see [RELEASING.md](RELEASING.md)) |

Actions are pinned to a commit SHA, with the version as a comment; update them together with the comment.

The contract tests are skipped in CI: they need a running Postilio and a test key. Run them locally before a release.
Never attach a self-hosted runner to this public repository: a pull request from a fork would run its code on that
machine.
