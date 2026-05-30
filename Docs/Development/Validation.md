# Validation

## Standard

```bash
dotnet build ChangeTrace.slnx
dotnet test Tests/ChangeTrace.Tests.csproj --no-build
```

In restricted shells, `dotnet test` may need permission to create a local socket.

The test project lives at:

```text
Tests/ChangeTrace.Tests.csproj
```

## Task

```bash
task build
task check
```

Useful tasks:

| Task | Purpose |
| --- | --- |
| `task build` | Restore and build. |
| `task check` | Release build, tools, and asset validation. |
| `task benchmark` | Rendering benchmarks. |
| `task publish` | Publish a runtime. |

## Manual Checks

CLI:

```bash
./changetrace --help
./changetrace ws ls
```

Workspace/export:

```bash
./changetrace ws current
./changetrace ws tl
```

Player:

```bash
./changetrace ws play -w -s
```
