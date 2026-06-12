[ADR Home](../README.md) | [Category Index](./README.md) | [Next](029-support-local-auth-sessions.md)

## [028] Keep Authentication In Credentialtrace

*2026-02* | Status: accepted

**Context:**

Authentication state is reused across export, enrichment, and workspace oriented commands, but it is not part of timeline construction itself. The system needs one place that owns provider definitions, sessions, and token persistence without pushing those concerns into export orchestration or rendering.

**Problem:**

Without a dedicated auth module, access tokens tend to get threaded through command handlers, exporter entry points, and provider clients as parameters. That makes session reuse awkward, spreads security sensitive logic across unrelated layers, and makes it hard to restore a consistent local auth context between commands.

**Decision:**

Auth flows, sessions, tokens, and providers live in the `CredentialTrace` module. Export and workspace code consume auth state through contracts instead of owning token lifecycle or provider specific session behavior directly.

**Rejected:**

- Keeping authentication logic inside the exporter.
- Passing tokens through every layer as call parameters.
- Splitting auth and workspace state across unrelated and uncoordinated storage locations.
- Keeping local context only in process memory or only in CLI arguments.

**Consequences:**

Export consumes sessions through contracts, and auth reuse becomes simpler across commands. The tradeoff is that `CredentialTrace` becomes a security sensitive module whose storage format, migrations, and provider session behavior must be maintained carefully.

[ADR Home](../README.md) | [Category Index](./README.md) | [Next](029-support-local-auth-sessions.md)
