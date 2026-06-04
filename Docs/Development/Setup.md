# Setup

## Requirements

| Tool | Why |
| --- | --- |
| .NET 10 SDK | Build, tests, CLI. |
| Git | Repositories and workflow. |
| Task | Optional command shortcuts. |
| Graphical session | OpenTK player and render windows. |

## Build

```bash
dotnet restore ChangeTrace.slnx
dotnet build ChangeTrace.slnx
```

Or:

```bash
task build
```

## Local CLI

Docs use:

```bash
./changetrace --help
```

In a dev build, this can point to the binary under `bin/Debug/net10.0/`.

## Global CLI

For local development, ChangeTrace can also be installed as a .NET global tool:

```bash
task tool:install
```

After installation, run it from any directory:

```bash
changetrace --help
```

If the tool is already installed and you rebuilt the package, update it with:

```bash
task tool:update
```

Uninstall it with:

```bash
task tool:uninstall
```

Make sure the .NET tools directory is on `PATH`:

```bash
export PATH="$PATH:$HOME/.dotnet/tools"
```

## GitHub Release CLI

GitHub releases publish runtime zip files and a `.nupkg` .NET tool package.
After downloading the package from the release assets, install it from the
download directory:

```bash
mkdir -p ~/.changetrace/tool-feed
cp ChangeTrace.*.nupkg ~/.changetrace/tool-feed/
dotnet tool install --global --add-source ~/.changetrace/tool-feed --ignore-failed-sources ChangeTrace
```

Then run:

```bash
changetrace --help
```

For a newer release, copy the new `.nupkg` into the same feed and update:

```bash
dotnet tool update --global --add-source ~/.changetrace/tool-feed --ignore-failed-sources ChangeTrace
```

## Local Data

```text
~/.changetrace/
workspaces/{organization}/{workspace}/timelines/{repository}/
```

Auth files are local to the user. Treat them as sensitive data.
