[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](053-use-provider-detection-from-repository-urls.md) | [Next](094-use-device-flow-where-providers-support-it.md)

## [093] Resolve Timeline Enrichers By Provider

*2026-06* | Status: accepted

**Context:**

This decision clarifies where the system recognizes and handles differences between hosting providers. The goal is to avoid a situation in which GitHub, GitLab, or custom OIDC leak as conditions into the entire export path and CLI.

**Problem:**

The exporter needs to resolve the correct enricher from the provider and the active call configuration.
The main concern in this area is separating what is common to ChangeTrace from what depends on a specific hosting provider and its API limits.

**Decision:**

Enricher selection goes through a provider-aware resolver.
This decision pushes provider differences behind specialized contracts instead of spreading them through the export path and CLI.

**Rejected:**

- Manual `if/else` branching in the export path.
- Providing no enrichment for providers other than GitHub.
- Flattening all providers to the same lowest common denominator at the cost of losing their real constraints.
- Spreading provider-specific logic across the exporter, prompts, auth flows, and history-reading layers.

**Consequences:**

Export stays extensible, but fallbacks and unsupported cases must remain explicit.
This improves extensibility and makes behavior for new providers more predictable, but explicit fallbacks and unsupported scenarios still have to be maintained.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](053-use-provider-detection-from-repository-urls.md) | [Next](094-use-device-flow-where-providers-support-it.md)
