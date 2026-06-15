[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](030-use-encrypted-local-token-storage.md) | [Next](035-persist-active-workspace-context.md)

## [034] Model Organizations Separately From Workspaces

*2026-02* | Status: accepted

**Context:**

The local workspace model has to represent both who owns a repository and where the operator is currently working. Provider organizations and local workspaces overlap, but they do not mean the same thing, and treating them as one concept makes local state harder to manage as the number of repositories grows.

**Problem:**

Repositories are naturally grouped by organization, but operators also need a local boundary for selecting active context, storing timelines, and reusing settings across commands. A single flat project catalog does not capture that distinction, and using provider organization as the only grouping model leaks remote host assumptions into local storage.

**Decision:**

Organizations and workspaces are modeled as separate local profiles. Organizations represent ownership boundaries, while workspaces represent the operator's local working boundary and active context.

**Rejected:**

- One flat catalog of projects.
- Using the provider organization as the only local model.
- Splitting auth and workspace state across unrelated and uncoordinated storage locations.
- Keeping local context only in process memory or only in CLI arguments.

**Consequences:**

The local model matches repository ownership and operator context more closely, which makes navigation and persistence behavior easier to reason about. The cost is additional metadata and one more layer of local indirection that storage and migration code must keep consistent.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](030-use-encrypted-local-token-storage.md) | [Next](035-persist-active-workspace-context.md)
