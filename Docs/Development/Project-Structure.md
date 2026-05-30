# Project Structure

## Main Paths

| Path | Role |
| --- | --- |
| `src/Cli` | Commands, handlers, prompts. |
| `src/Configuration` | DI and app startup. |
| `src/Core` | Domain model and timeline. |
| `src/CredentialTrace` | Auth, profiles, workspace storage. |
| `src/GIt` | Git reading and export. |
| `src/Graphics` | OpenTK, GPU, shaders. |
| `src/Player` | Timeline playback. |
| `src/Rendering` | Scene and render commands. |
| `Tools/` | Helper tools project. |
| `Benchmarks/` | Benchmarks project. |
| `Tests/` | Test project. |
| `Docs/` | Documentation. |

## CLI Pattern

```text
src/Cli/Commands/**/*
src/Cli/Handlers/**/*
```

Commands define syntax. Handlers do the work.

## Project Layout

Sibling projects live at the repository root:

```text
Tools/ChangeTrace.Tools.csproj
Benchmarks/ChangeTrace.Benchmarks.csproj
Tests/ChangeTrace.Tests.csproj
```

Keep test files directly under `Tests/` unless a larger test area needs its own folder.

## Workspace Timelines

```text
src/CredentialTrace/Interfaces/IWorkspaceTimelineStorage.cs
src/CredentialTrace/Services/WorkspaceTimelineStorage.cs
```

Layout:

```text
workspaces/{organization}/{workspace}/timelines/{repository}/
```
