# Validation

## Standard

```bash
dotnet build ChangeTrace.slnx
dotnet test Tests/ChangeTrace.Tests.csproj
```

In restricted shells, `dotnet test` may need permission to create a local socket.

The test project lives at:
 
```text
Tests/ChangeTrace.Tests.csproj
```

The regression suite is CPU only. It covers domain value objects, timeline
normalization, aggregation, player state and boundary behavior, profile and
workspace persistence, selected non-interactive CLI profile logic, and
CPU side rendering state assembly. It must not open OpenTK windows, require GPU
access, or run full rendering loops.

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
| `task publish` | Publish runtime. |

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
