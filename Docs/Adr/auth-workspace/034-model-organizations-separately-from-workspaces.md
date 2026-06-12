[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](030-use-encrypted-local-token-storage.md) | [Next](035-persist-active-workspace-context.md)

## [034] Model Organizations Separately From Workspaces

*2026-02* | Status: accepted

**Context:**

This category concerns everything that forms durable local execution context: from auth sessions and profiles to the place where the timeline is stored. This is not a CLI ergonomics detail, but a real local-storage contract.

**Problem:**

Repositories are naturally grouped by organizations and workspaces.
This group of decisions defines the local execution state: sessions, profiles, identities, active workspace context, and the physical location of stored timelines.

**Decision:**

Organizations and workspaces are modeled as separate local profiles.
Authentication and workspace state are not an export detail; they form a separate layer responsible for restoring local working context between application runs.

**Rejected:**

- Jeden plaski katalog projektow.
- Using the provider organization as the only local model.
- Splitting auth and workspace state across unrelated and uncoordinated storage locations.
- Keeping local context only in process memory or only in CLI arguments.

**Consequences:**

Model is blizszy rzeczywistej strukturze organizacyjnej, but storage ma wiecej metadanych.
Local storage becomes part of the system contract, so its consistency, migrations, and security have to be maintained carefully.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](030-use-encrypted-local-token-storage.md) | [Next](035-persist-active-workspace-context.md)
