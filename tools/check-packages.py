"""Checks the packed packages: the expected files and dependencies per target, the XML docs and readme, symbols, and
no API key or webhook secret anywhere. Usage: check-packages.py <directory> <version>"""
import re
import sys
import zipfile
from pathlib import Path

directory, version = Path(sys.argv[1]), sys.argv[2]
targets = ["net8.0", "net10.0"]
secret = re.compile(rb"pk_(live|test)_[A-Za-z0-9]{32}|whsec_[A-Za-z0-9+/]{20,}")
expected = {
    "Postilio.Client": {"Microsoft.Extensions.Http"},
    "Postilio.Client.AspNetCore": {"Postilio.Client"},
}
problems = []

for package, dependencies in expected.items():
    path = directory / f"{package}.{version}.nupkg"
    if not path.exists():
        problems.append(f"{path.name}: missing")
        continue
    if not (directory / f"{package}.{version}.snupkg").exists():
        problems.append(f"{package}: no symbol package")
    with zipfile.ZipFile(path) as nupkg:
        names = set(nupkg.namelist())
        nuspec = nupkg.read(f"{package}.nuspec").decode()
        for target in targets:
            for suffix in (".dll", ".xml"):
                if f"lib/{target}/{package}{suffix}" not in names:
                    problems.append(f"{package}: no lib/{target}/{package}{suffix}")
            group = re.search(rf'<group targetFramework="{re.escape(target)}">(.*?)</group>', nuspec, re.S)
            found = set(re.findall(r'<dependency id="([^"]+)"', group.group(1))) if group else set()
            if found != dependencies:
                problems.append(f"{package} {target}: dependencies {sorted(found)}, expected {sorted(dependencies)}")
        if "README.md" not in names or "<readme>README.md</readme>" not in nuspec:
            problems.append(f"{package}: no readme")
        if '<license type="expression">MIT</license>' not in nuspec:
            problems.append(f"{package}: no MIT license expression")
        for tag in ("description", "repository", "projectUrl"):
            if f"<{tag}" not in nuspec:
                problems.append(f"{package}: no <{tag}> in the nuspec")
        unexpected = [n for n in names if re.search(r"(Tests?\.dll|\.pdb|appsettings|\.env)$", n, re.I)]
        if unexpected:
            problems.append(f"{package}: unexpected files {unexpected}")
        for name in names:
            if secret.search(nupkg.read(name)):
                problems.append(f"{package}: something that looks like a key or secret in {name}")
    print(f"{path.name}: {len(names)} files")

if problems:
    print("\n".join(problems), file=sys.stderr)
    sys.exit(1)
