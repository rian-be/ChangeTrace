# Organizations

`org` also has the full alias `organization`.

```bash
./changetrace org create microsoft -p github
./changetrace org list
./changetrace org remove microsoft
```

## Commands

| Command | Purpose |
| --- | --- |
| `org create <name> -p <provider>` | Create an organization. |
| `org list` | List organizations. |
| `org remove <name>` | Remove an organization. |

Provider can be `github`.

`org remove` deletes the organization profile. Use `-y` only when the target name is clear.
