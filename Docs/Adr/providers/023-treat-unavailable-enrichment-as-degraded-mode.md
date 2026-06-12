[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](022-skip-unavailable-github-pr-data-without-failing-export.md) | [Next](033-support-provider-aware-login-flows.md)

## [023] Treat Unavailable Enrichment As Degraded Mode

*2026-06* | Status: accepted

**Context:**

This decision clarifies where the system recognizes and handles differences between hosting providers. The goal is to avoid a situation in which GitHub, GitLab, or custom OIDC leak as conditions into the entire export path and CLI.

**Problem:**

Optional provider data should not prevent persistence of the main timeline.
The main concern in this area is separating what is common to ChangeTrace from what depends on a specific hosting provider and its API limits.

**Decision:**

Brak enrichment is obslugiwany jako tryb zdegradowany, gdy main eksport can sie udac.
This decision pushes provider differences behind specialized contracts instead of spreading them through the export path and CLI.

**Rejected:**

- Failing fast for every unavailable provider datum.
- Ciche pomijanie bez sygnalu dla layers wywolujacej.
- Flattening all providers to the same lowest common denominator at the cost of losing their real constraints.
- Spreading provider-specific logic across the exporter, prompts, auth flows, and history-reading layers.

**Consequences:**

Export becomes more resilient, but consumers must understand when sidecars are missing.
This improves extensibility and makes behavior for new providers more predictable, but explicit fallbacks and unsupported scenarios still have to be maintained.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](022-skip-unavailable-github-pr-data-without-failing-export.md) | [Next](033-support-provider-aware-login-flows.md)
