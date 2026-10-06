#!/usr/bin/env bash
# The local check before a push or a release: build with warnings as errors, run the tests, pack, and inspect the
# packages. The contract tests run too when POSTILIO_CONTRACT_* is set (see CONTRIBUTING.md); otherwise they are skipped.
set -euo pipefail
cd "$(dirname "$0")"

dotnet restore Postilio.slnx
dotnet build Postilio.slnx -c Release --no-restore -warnaserror
dotnet test --solution Postilio.slnx -c Release --no-build

version=$(dotnet msbuild src/Postilio.Client/Postilio.Client.csproj -getProperty:Version)
out="artifacts/packages/$version"
dotnet pack Postilio.slnx -c Release --no-build -o "$out" -warnaserror
python3 tools/check-packages.py "$out" "$version"
echo "OK: $out"
