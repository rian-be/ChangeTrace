[ADR Home](../README.md) | [Category Index](./README.md) | [Next](029-support-local-auth-sessions.md)

## [028] Keep Authentication In Credentialtrace

*2026-02* | Status: accepted

**Context:**

This category concerns everything that forms durable local execution context: from auth sessions and profiles to the place where the timeline is stored. This is not a CLI ergonomics detail, but a real local-storage contract.

**Problem:**

Provider authentication is a separate problem from export and rendering.
This group of decisions defines the local execution state: sessions, profiles, identities, active workspace context, and the physical location of stored timelines.

**Decision:**

Auth flows, sessions, tokens, and providers live in the `CredentialTrace` module.
Authentication and workspace state are not an export detail; they form a separate layer responsible for restoring local working context between application runs.

**Rejected:**

- Uwierzytelnianie wewnatrz eksportera.
- Przekazywanie tokenow przez wszystkie layers.
- Splitting auth and workspace state across unrelated and uncoordinated storage locations.
- Keeping local context only in process memory or only in CLI arguments.

**Consequences:**

Export consumes sessions through contracts, but `CredentialTrace` becomes a security-sensitive module.
Local storage becomes part of the system contract, so its consistency, migrations, and security have to be maintained carefully.

[ADR Home](../README.md) | [Category Index](./README.md) | [Next](029-support-local-auth-sessions.md)
