[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](028-keep-authentication-in-credentialtrace.md) | [Next](030-use-encrypted-local-token-storage.md)

## [029] Support Local Auth Sessions

*2026-02* | Status: accepted

**Context:**

This category concerns everything that forms durable local execution context: from auth sessions and profiles to the place where the timeline is stored. This is not a CLI ergonomics detail, but a real local-storage contract.

**Problem:**

The calling layer should not require the token to be entered again for every export.
This group of decisions defines the local execution state: sessions, profiles, identities, active workspace context, and the physical location of stored timelines.

**Decision:**

Auth sessions are persisted locally and can be reused.
Authentication and workspace state are not an export detail; they form a separate layer responsible for restoring local working context between application runs.

**Rejected:**

- Passing the token only as a CLI argument.
- A remote ChangeTrace session backend.
- Splitting auth and workspace state across unrelated and uncoordinated storage locations.
- Keeping local context only in process memory or only in CLI arguments.

**Consequences:**

The authorization flow becomes shorter, but local auth files require protection.
Local storage becomes part of the system contract, so its consistency, migrations, and security have to be maintained carefully.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](028-keep-authentication-in-credentialtrace.md) | [Next](030-use-encrypted-local-token-storage.md)
