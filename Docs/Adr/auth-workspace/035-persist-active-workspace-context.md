[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](034-model-organizations-separately-from-workspaces.md) | [Next](036-use-ulid-for-local-identities.md)

## [035] Persist Active Workspace Context

*2026-02* | Status: accepted

**Context:**

Export, playback, and repository commands often target the same workspace across multiple runs. The system already persists related local state such as profiles and sessions, so workspace selection needs the same durability if commands are to operate against one consistent local context.

**Problem:**

Export and playback should be able to reuse the current workspace without requiring the caller to repeat organization and workspace arguments on every command. If active workspace selection is transient, command behavior becomes harder to predict and higher level flows must keep reconstructing context that the local environment already knows.

**Decision:**

The active workspace is persisted as part of local execution state. Workspace selection lives alongside related local auth and profile state so that application runs can restore one coherent working context.

**Rejected:**

- Requiring `org` and `workspace` in every command.
- Keeping context only in process memory.
- Splitting auth and workspace state across unrelated and uncoordinated storage locations.
- Keeping local context only in process memory or only in CLI arguments.

**Consequences:**

Commands can default to a stable workspace context without repeated selection arguments. The tradeoff is that the stored context becomes part of the durable local contract, so visibility, consistency, migration, and reset behavior have to stay explicit.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](034-model-organizations-separately-from-workspaces.md) | [Next](036-use-ulid-for-local-identities.md)
