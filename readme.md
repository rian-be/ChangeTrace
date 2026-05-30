# ChangeTrace

<table width="100%">
  <tr>
    <td align="left">
      <a href="https://learn.microsoft.com/dotnet/csharp/"><img alt="Language: C#" src="https://img.shields.io/badge/Language-C%23-239120?style=flat-square&logo=csharp&logoColor=white"></a>
      <a href="https://dotnet.microsoft.com/"><img alt="Runtime: .NET 10" src="https://img.shields.io/badge/Runtime-.NET%2010-512BD4?style=flat-square&logo=dotnet&logoColor=white"></a>
      <a href="LICENSE"><img alt="License: MIT" src="https://img.shields.io/badge/License-MIT-2EA043?style=flat-square"></a>
    </td>
    <td align="right">
      <a href="https://github.com/rian-be/ChangeTrace/actions/workflows/publish-attested.yml?query=branch%3Adevelop"><img alt="Publish Attested: develop" src="https://img.shields.io/github/actions/workflow/status/rian-be/ChangeTrace/publish-attested.yml?branch=develop&style=flat-square&label=Publish%20Attested%20(develop)"></a>
      <a href="https://github.com/rian-be/ChangeTrace/actions/workflows/publish-attested.yml?query=branch%3Amain"><img alt="Publish Attested: main" src="https://img.shields.io/github/actions/workflow/status/rian-be/ChangeTrace/publish-attested.yml?branch=main&style=flat-square&label=Publish%20Attested%20(main)"></a>
    </td>
  </tr>
</table>

ChangeTrace turns Git repositories into timeline files and lets you inspect them later.

It reads repository history, branch activity, merges, and other Git events, then turns them into a timeline you can export, inspect, and replay locally. Instead of digging through raw logs or switching between tools, you get one app that combines analysis with an OpenGL visualization layer.

> [!IMPORTANT]
> ChangeTrace keeps repository history, timeline data, workspace profiles, and local auth in one place.

> [!WARNING]
> This is an early experimental version. The core workflow exists, but the codebase and UX are still being stabilized.

Use it when you want a repeatable way to save repository history, check important moments, or use the same timeline in different views. It is made for local use, so exported data, auth sessions, workspace state, and visual state stay on your machine.

The usual flow is simple:

- create or select a workspace
- export a repository into a portable `.gittrace` timeline
- open that timeline later from the workspace
- keep auth and workspace data local instead of depending on a remote service

## Project State

ChangeTrace is currently in `State 1`.

This phase is about making the core workflow stable, keeping export and inspection smooth, and improving the CLI/player experience around workspaces.

See [Project State](Docs/Project-State.md) for the full breakdown of the current phase and next steps. For artifact provenance and verification, see [Publish Attested](Docs/Publish-Attested.md).

## What It Does

ChangeTrace analyzes repository history and produces a structured timeline that can be used for:

- commit and branch analysis
- timeline inspection
- debug and playback views
- workspace and organization management around traced repositories

## Commands

ChangeTrace exposes entry points for export, inspection, authentication, organization profiles, workspace management, and playback.

The main entry points are `export`, `show`, `auth`, `org`, and `workspace`.

Typical workspace usage:

```bash
./changetrace auth login github
./changetrace org create microsoft -p github
./changetrace ws create msquic --org microsoft
./changetrace ws use microsoft msquic
./changetrace export https://github.com/microsoft/msquic.git
./changetrace ws play
```

Interactive workspace and timeline selection:

```bash
./changetrace ws use
./changetrace ws play -w -s
```

Direct file export is still available when you want a specific output path:

```bash
./changetrace export https://github.com/microsoft/WSL.git -o timeline.gittrace
./changetrace show timeline.gittrace
```

For local development, build the project and use `./changetrace` as the local command. It can point to the built binary under `bin/Debug/net10.0/`.

```bash
dotnet build ChangeTrace.slnx
./changetrace --help
```

For the full command guide, see [CLI Guide](Docs/Guides/Cli/README.md).

## Workspace Storage

When `export` runs without `--output`, it writes to the active workspace:

```text
workspaces/{organization}/{workspace}/timelines/{repository}/{timestamp}-{ulid}.gittrace
```

Each workspace export also writes metadata next to the timeline:

```text
*.gittrace.metadata.json
```

Use `ws current`, `ws timelines`, and `ws play` to inspect and replay workspace timelines.

## Documentation

| Topic | Link |
| --- | --- |
| Documentation index | [Docs](Docs/README.md) |
| CLI overview | [CLI Guide](Docs/Guides/Cli/README.md) |
| Export | [Export](Docs/Guides/Cli/Export.md) |
| Workspaces | [Workspace](Docs/Guides/Cli/Workspace.md) |
| Organizations | [Organizations](Docs/Guides/Cli/Organizations.md) |
| Auth | [Auth](Docs/Guides/Cli/Auth.md) |
| Development | [Development](Docs/Development/README.md) |
| Project structure | [Project Structure](Docs/Development/Project-Structure.md) |
| Validation | [Validation](Docs/Development/Validation.md) |

## Local Data

ChangeTrace stores local auth data under the user profile:

- On Unix-like systems the data lives under `~/.changetrace/`.
- On Windows it uses the equivalent user profile directory.
- `auth.json` contains session metadata and encrypted tokens.
- `auth.key` contains the local key used by the token store.

Workspace timelines are stored under the app workspace storage:

```text
workspaces/{organization}/{workspace}/timelines/{repository}/
```

This protects against accidental plaintext exposure in `auth.json`, but it is not a full operating-system keychain. A process running as the same user can still read both files.

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE).

## Code of Conduct

Please read and follow the [Code of Conduct](CODE_OF_CONDUCT.md).

## Security

For vulnerability reporting, see [Security Policy](SECURITY.md).

---

ChangeTrace is built for local repository analysis and repeatable timeline workflows.

[Back to top](#changetrace)
