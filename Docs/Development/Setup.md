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

## Local Data

```text
~/.changetrace/
workspaces/{organization}/{workspace}/timelines/{repository}/
```

Auth files are local to the user. Treat them as sensitive data.
