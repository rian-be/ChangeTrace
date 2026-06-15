[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](035-persist-active-workspace-context.md) | [Next](090-store-workspace-timelines-with-metadata.md)

## [036] Use ULID For Local Identities

*2026-02* | Status: accepted

**Context:**

This category concerns everything that forms durable local execution context: from auth sessions and profiles to the place where the timeline is stored. This is not a CLI ergonomics detail, but a real local storage contract.

**Problem:**

Profiles, workspaces, and exports need stable local identifiers.
This group of decisions defines the local execution state: sessions, profiles, identities, active workspace context, and the physical location of stored timelines.

**Decision:**

Local identifiers use ULID.
Authentication and workspace state are not an export detail; they form a separate layer responsible for restoring local working context between application runs.

**Rejected:**

- Random unreadable GUIDs everywhere.
- Identifying everything only through logical names.
- Splitting auth and workspace state across unrelated and uncoordinated storage locations.
- Keeping local context only in process memory or only in CLI arguments.

**Consequences:**

Identifiers are stable and time sortable, but they require serializers and converters.
Local storage becomes part of the system contract, so its consistency, migrations, and security have to be maintained carefully.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](035-persist-active-workspace-context.md) | [Next](090-store-workspace-timelines-with-metadata.md)
