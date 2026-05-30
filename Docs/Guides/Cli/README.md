# ChangeTrace CLI

Examples use the local command `./changetrace`.

## Quick Start

```bash
./changetrace auth login github
./changetrace org create microsoft -p github
./changetrace ws create msquic --org microsoft
./changetrace ws use microsoft msquic
./changetrace export https://github.com/microsoft/msquic.git
./changetrace ws play
```

Interactive flow:

```bash
./changetrace ws use
./changetrace ws play -w -s
```

## Sections

| Section | File |
| --- | --- |
| Export and `.gittrace` | [Export](Export.md) |
| Workspaces | [Workspace](Workspace.md) |
| Organizations | [Organizations](Organizations.md) |
| Auth | [Auth](Auth.md) |
| Dev debug | [Debug](../../Development/Debug.md) |

## Commands

| Command | Alias |
| --- | --- |
| `export <repository>` | |
| `show <file>` | |
| `workspace create <name>` | `ws create` |
| `workspace list` | `ws list`, `ws ls` |
| `workspace use [org] [name]` | `ws use`, `ws switch`, `ws select` |
| `workspace current` | `ws current`, `ws status`, `ws ctx` |
| `workspace timelines` | `ws timelines`, `ws timeline`, `ws tl` |
| `workspace play` | `ws play` |
| `workspace remove <name>` | `ws remove` |
| `org create <name>` | `organization create` |
| `org list` | `organization list` |
| `org remove <name>` | `organization remove` |
| `auth login <provider>` | |
| `auth list` | |
| `auth logout [provider]` | |
| `debdev player <file>` | |
| `debdev render <file>` | |
| `debdev window <file>` | |

`export` without `--output` writes to the active workspace. Set it with `ws use`.
