[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](034-model-organizations-separately-from-workspaces.md) | [Next](036-use-ulid-for-local-identities.md)

## [035] Persist Active Workspace Context

*2026-02* | Status: accepted

**Context:**

This category concerns everything that forms durable local execution context: from auth sessions and profiles to the place where the timeline is stored. This is not a CLI ergonomics detail, but a real local-storage contract.

**Problem:**

Export and play commands should operate in the current context without repeating arguments.
This group of decisions defines the local execution state: sessions, profiles, identities, active workspace context, and the physical location of stored timelines.

**Decision:**

The active workspace is persisted locally.
Authentication and workspace state are not an export detail; they form a separate layer responsible for restoring local working context between application runs.

**Rejected:**

- Requiring `org` and `workspace` in every command.
- Keeping context only in process memory.
- Splitting auth and workspace state across unrelated and uncoordinated storage locations.
- Keeping local context only in process memory or only in CLI arguments.

**Consequences:**

CLI is wygodniejsze, but it is necesarery to jasno pokazywac aktualny kontekst.
Local storage becomes part of the system contract, so its consistency, migrations, and security have to be maintained carefully.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](034-model-organizations-separately-from-workspaces.md) | [Next](036-use-ulid-for-local-identities.md)
