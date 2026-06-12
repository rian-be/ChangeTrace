[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](023-treat-unavailable-enrichment-as-degraded-mode.md) | [Next](053-use-provider-detection-from-repository-urls.md)

## [033] Support Provider Aware Login Flows

*2026-06* | Status: accepted

**Context:**

This decision clarifies where the system recognizes and handles differences between hosting providers. The goal is to avoid a situation in which GitHub, GitLab, or custom OIDC leak as conditions into the entire export path and CLI.

**Problem:**

Rozne serwisy wspieraja rozne mechanizmy logowania.
The main concern in this area is separating what is common to ChangeTrace from what depends on a specific hosting provider and its API limits.

**Decision:**

Login selects a provider-specific flow instead of one global scheme.
This decision pushes provider differences behind specialized contracts instead of spreading them through the export path and CLI.

**Rejected:**

- Jeden GitHub device flow dla wszystkich.
- Token-only auth for providers without device flow.
- Flattening all providers to the same lowest common denominator at the cost of losing their real constraints.
- Spreading provider-specific logic across the exporter, prompts, auth flows, and history-reading layers.

**Consequences:**

Auth becomes more flexible, but the CLI must explicitly handle different authorization scenarios.
This improves extensibility and makes behavior for new providers more predictable, but explicit fallbacks and unsupported scenarios still have to be maintained.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](023-treat-unavailable-enrichment-as-degraded-mode.md) | [Next](053-use-provider-detection-from-repository-urls.md)
