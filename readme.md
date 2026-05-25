# ChangeTrace

<table  widtxh="100%">
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
> ChangeTrace keeps repository history, timeline data, and local auth in one place.

> [!WARNING]
> This is an early experimental version. The core workflow exists, but the codebase is still being stabilized.

Use it when you want a repeatable way to save repository history, check important moments, or use the same timeline in different views. It is made for local use, so the exported data, auth sessions, workspace state, and visual state stay on your machine.

The usual flow is simple:

- export a repository into a portable `.gittrace` file
- open that file later to inspect the timeline
- keep auth and workspace data local instead of depending on a remote service

## Project State

ChangeTrace is currently in `State 1`.

This phase is about making the core code stable, keeping export and inspection smooth, and making sure the current behavior stays predictable.

See [Project State](Docs/Project-State.md) for the full breakdown of the current phase and the next step.
For artifact provenance and verification, see [Publish Attested](Docs/Publish-Attested.md).

## What it does

ChangeTrace analyzes repository history and produces a structured timeline that can be used for:

- commit and branch analysis
- timeline inspection
- debug and playback views
- workspace and organization management around traced repositories

## Commands

ChangeTrace exposes a small set of entry points that cover export, inspection, authentication, and workspace management.

The main entry points are `export`, `show`, `auth`, `org`, and `workspace`.

Typical usage:

```bash
./changetrace export https://github.com/user/repo -o timeline.gittrace
./changetrace show timeline.gittrace
./changetrace auth login github
./changetrace auth list
./changetrace auth logout github
./changetrace org list --provider github
./changetrace workspace list --org acme
```

For local development, replace `./changetrace` with `dotnet run --`.

## Project Structure

The repository is split into a few clear layers:

- `src/Core` contains the event model, rules, and shared processing
- `src/Configuration` contains app settings, converters, and service discovery helpers
- `src/GIt` reads repositories, builds timelines, and enriches data
- `src/CredentialTrace` handles auth, sessions, and local storage
- `src/Rendering` builds the timeline scene and state
- `src/Graphics` owns the rendering runtime, shaders, and GPU helpers
- `src/Player` handles timeline playback flow and speed control
- `src/Cli` wires commands to handlers
- `Tools` contains repository utilities and asset maintenance scripts

## Local Data

ChangeTrace stores local auth data under the user profile:

- On Unix like systems the data lives under `~/.changetrace/`.
- On Windows it uses the equivalent user profile directory.
- `auth.json` contains session metadata and encrypted tokens.
- `auth.key` contains the local key used by the token store.

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
