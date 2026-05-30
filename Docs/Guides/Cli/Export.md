# Export

## `export`

```bash
./changetrace export <repository>
./changetrace export <repository> -o ./file.gittrace
```

Examples:

```bash
./changetrace export https://github.com/microsoft/msquic.git
./changetrace export ../local-repo
./changetrace export https://github.com/microsoft/WSL.git -o ./wsl.gittrace
```

Without `--output`, the file is stored in the active workspace:

```text
workspaces/{organization}/{workspace}/timelines/{repository}/{timestamp}-{ulid}.gittrace
```

Timeline metadata is stored next to it:

```text
*.gittrace.metadata.json
```

Key options:

| Option | Meaning |
| --- | --- |
| `-o, --output <path>` | Write to a specific file. |
| `-r, --token <token>` | GitHub token. |
| `-v, --verbose` | More logs. |

## `show`

```bash
./changetrace show <file.gittrace>
```

For workspace timelines, `ws play` is usually easier.
