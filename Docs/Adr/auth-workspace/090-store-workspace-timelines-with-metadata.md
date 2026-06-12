[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](036-use-ulid-for-local-identities.md)

## [090] Store Workspace Timelines With Metadata

*2026-05* | Status: accepted

**Context:**

This category concerns everything that forms durable local execution context: from auth sessions and profiles to the place where the timeline is stored. This is not a CLI ergonomics detail, but a real local-storage contract.

**Problem:**

A timeline persisted in a workspace should be available for later discovery and description.
This group of decisions defines the local execution state: sessions, profiles, identities, active workspace context, and the physical location of stored timelines.

**Decision:**

Export into a workspace persists `.gittrace` together with adjacent metadata.
Authentication and workspace state are not an export detail; they form a separate layer responsible for restoring local working context between application runs.

**Rejected:**

- Only `.gittrace` files without an index.
- Globalny katalog timeline'ow bez podzialu na workspace.
- Splitting auth and workspace state across unrelated and uncoordinated storage locations.
- Keeping local context only in process memory or only in CLI arguments.

**Consequences:**

Workspace playback and timeline listing become possible, but the metadata must be preserved when files are moved.
Local storage becomes part of the system contract, so its consistency, migrations, and security have to be maintained carefully.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](036-use-ulid-for-local-identities.md)
