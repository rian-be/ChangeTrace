# Debug

`debdev` commands are for development. Use `ws play` for normal playback.

```bash
./changetrace debdev player <file.gittrace>
./changetrace debdev render <file.gittrace>
./changetrace debdev window <file.gittrace>
```

| Command | Purpose |
| --- | --- |
| `debdev player <file>` | Debug the player. |
| `debdev render <file>` | Debug rendering. |
| `debdev window <file>` | Debug the OpenTK window. |

`TimelineLoader` in `src/Cli/Commands/Debug/TimelineLoaderDebug.cs` is a helper, not a CLI command.
