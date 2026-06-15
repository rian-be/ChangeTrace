[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](036-use-ulid-for-local-identities.md)

## [090] Store Workspace Timelines With Metadata

*2026-05* | Status: accepted

**Context:**

Workspace exports are not useful if the timeline artifact can only be loaded by remembering its exact file path manually. Once timelines are stored under a workspace, the local store also needs enough adjacent metadata to support listing, discovery, and later selection.

**Problem:**

A timeline persisted in a workspace should remain discoverable and describable after the export command completes. If only the `.gittrace` payload is stored, later commands have to infer identity, provenance, and presentation details from file names or ad hoc scanning rules.

**Decision:**

Workspace export persists the `.gittrace` artifact together with adjacent metadata that describes the stored timeline. Timeline storage is treated as a local workspace contract rather than just a file drop performed by export.

**Rejected:**

- Only `.gittrace` files without an index.
- One global timeline directory with no workspace partitioning.
- Splitting auth and workspace state across unrelated and uncoordinated storage locations.
- Keeping local context only in process memory or only in CLI arguments.

**Consequences:**

Workspace playback and timeline listing can rely on explicit stored metadata instead of reconstructing context from file layout alone. The tradeoff is that metadata becomes part of the durable local contract and must stay in sync when timelines are moved, replaced, or migrated.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](036-use-ulid-for-local-identities.md)
