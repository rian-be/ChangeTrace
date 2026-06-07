# Auth

```bash
./changetrace auth login github
./changetrace auth login codeberg
./changetrace auth list
./changetrace auth logout github
./changetrace auth logout codeberg
```

## Commands

| Command | Purpose |
| --- | --- |
| `auth login [provider]` | Log in to a provider. `codeberg` opens a preset host picker. |
| `auth list` | List saved sessions. |
| `auth logout [provider]` | Remove saved login data. |

Supported providers include `github`, `gitlab`, `codeberg` (preset: `codeberg.org`), and `custom` for OIDC providers you define yourself.

Sessions are local to the current user. Do not treat them as a keychain.
