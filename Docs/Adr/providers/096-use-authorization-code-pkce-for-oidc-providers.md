[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](095-support-custom-oidc-providers-through-dynamic-discovery-and-local-callbacks.md)

## [096] Use Authorization Code PKCE For OIDC Providers

*2026-06* | Status: accepted

**Context:**

This decision clarifies where the system recognizes and handles differences between hosting providers. The goal is to avoid a situation in which GitHub, GitLab, or custom OIDC leak as conditions into the entire export path and CLI.

**Problem:**

Not every provider offers device flow, and custom OIDC requires a secure standards-based flow.
The main concern in this area is separating what is common to ChangeTrace from what depends on a specific hosting provider and its API limits.

**Decision:**

Codeberg and custom providers use authorization-code PKCE/OIDC.
This decision pushes provider differences behind specialized contracts instead of spreading them through the export path and CLI.

**Rejected:**

- Having no support for providers that do not offer device flow.
- Using passwords or long-lived tokens as a fallback.
- Flattening all providers to the same lowest common denominator at the cost of losing their real constraints.
- Spreading provider-specific logic across the exporter, prompts, auth flows, and history-reading layers.

**Consequences:**

The provider set expands, but OIDC configuration must remain well described and validated.
This improves extensibility and makes behavior for new providers more predictable, but explicit fallbacks and unsupported scenarios still have to be maintained.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](095-support-custom-oidc-providers-through-dynamic-discovery-and-local-callbacks.md)
