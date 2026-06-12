[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](029-support-local-auth-sessions.md) | [Next](034-model-organizations-separately-from-workspaces.md)

## [030] Use Encrypted Local Token Storage

*2026-02* | Status: accepted

**Context:**

This category concerns everything that forms durable local execution context: from auth sessions and profiles to the place where the timeline is stored. This is not a CLI ergonomics detail, but a real local storage contract.

**Problem:**

Provider tokens should not be stored in plain text files.
This group of decisions defines the local execution state: sessions, profiles, identities, active workspace context, and the physical location of stored timelines.

**Decision:**

The token store encrypts tokens and hardens auth files.
Authentication and workspace state are not an export detail; they form a separate layer responsible for restoring local working context between application runs.

**Rejected:**

- Plaintext `auth.json`.
- No session persistence and forced login on every use.
- Splitting auth and workspace state across unrelated and uncoordinated storage locations.
- Keeping local context only in process memory or only in CLI arguments.

**Consequences:**

The risk of accidental leakage is reduced, but this is not a full OS keychain.
Local storage becomes part of the system contract, so its consistency, migrations, and security have to be maintained carefully.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](029-support-local-auth-sessions.md) | [Next](034-model-organizations-separately-from-workspaces.md)
