[ADR Home](../README.md)

# auth workspace

This category captures authentication, profiles, workspaces, and local execution state persisted between runs.

## Scope

Look here for auth sessions, token storage, profile/workspace modeling, active context persistence, and workspace timeline storage.

## Responsibility Boundaries

This layer owns local operational state. It should not redefine timeline semantics or export execution.

## How To Start Reading

Start here when changing local auth, workspace context, or persisted operator state.

## ADR List

| ADR | Title |
| --- | --- |
| [028-keep-authentication-in-credentialtrace.md](./028-keep-authentication-in-credentialtrace.md) | Keep Authentication In Credentialtrace |
| [029-support-local-auth-sessions.md](./029-support-local-auth-sessions.md) | Support Local Auth Sessions |
| [030-use-encrypted-local-token-storage.md](./030-use-encrypted-local-token-storage.md) | Use Encrypted Local Token Storage |
| [034-model-organizations-separately-from-workspaces.md](./034-model-organizations-separately-from-workspaces.md) | Model Organizations Separately From Workspaces |
| [035-persist-active-workspace-context.md](./035-persist-active-workspace-context.md) | Persist Active Workspace Context |
| [036-use-ulid-for-local-identities.md](./036-use-ulid-for-local-identities.md) | Use ULID For Local Identities |
| [090-store-workspace-timelines-with-metadata.md](./090-store-workspace-timelines-with-metadata.md) | Store Workspace Timelines With Metadata |
