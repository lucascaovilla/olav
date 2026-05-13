#!/usr/bin/env bash
set -e

VERSION=$(grep -oP '(?<=<Version>)[^<]+' src/Olav.Cli/Olav.Cli.csproj)

find . -type d \( -name bin -o -name obj -o -name nupkg \) -exec rm -rf {} +
dotnet pack src/Olav.Cli/Olav.Cli.csproj -c Release -o ./nupkg
dotnet tool uninstall -g olav.cli || true
dotnet tool install -g --add-source ./nupkg olav.cli --version "$VERSION"

echo "Installed olav version $VERSION"
