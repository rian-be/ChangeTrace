# Workspace

`workspace` has the short alias `ws`.

```bash
./changetrace ws <command>
```

## Flow

```bash
./changetrace org create microsoft -p github
./changetrace ws create wsl --org microsoft
./changetrace ws use microsoft wsl
./changetrace export https://github.com/microsoft/WSL.git
./changetrace ws play
```

Interactive:

```bash
./changetrace ws use
./changetrace ws play -w -s
```

## Commands

| Command | Alias | Purpose |
| --- | --- | --- |
| `ws create <name> --org <org>` | | Create a workspace. |
| `ws list` | `ws ls` | List workspaces. |
| `ws use [org] [name]` | `ws switch`, `ws select` | Set the active workspace. |
| `ws current` | `ws status`, `ws ctx` | Show the active workspace. |
| `ws timelines` | `ws timeline`, `ws tl` | List stored timelines. |
| `ws play` | | Open a timeline in the player. |
| `ws remove <name> --org <org>` | | Remove a workspace profile. |

## Useful Options

| Command | Option | Meaning |
| --- | --- | --- |
| `ws list` | `--org <org>` | Filter by organization. |
| `ws play` | `-r, --repo <repo>` | Pick a repo, for example `msquic` or `microsoft/wsl`. |
| `ws play` | `-s, --select` | Select a timeline from a list. |
| `ws play` | `-w, --workspace` | Select a workspace before playing. |
| `ws remove` | `-y, --yes` | Skip confirmation. |

`ws play` needs a graphical session because it opens an OpenTK window.
