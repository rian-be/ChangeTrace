# Auth

```bash
./changetrace auth login github
./changetrace auth list
./changetrace auth logout github
```

## Commands

| Command | Purpose |
| --- | --- |
| `auth login <provider>` | Log in to a provider. |
| `auth list` | List saved sessions. |
| `auth logout [provider]` | Remove saved login data. |

Sessions are local to the current user. Do not treat them as a keychain.
