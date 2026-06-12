[ADR Home](../README.md) | [Category Index](./README.md) | [Next](021-make-enrichment-optional-per-export.md)

## [020] Keep Provider Specific Enrichment Behind Interfaces

*2026-06* | Status: accepted

**Context:**

This decision clarifies where the system recognizes and handles differences between hosting providers. The goal is to avoid a situation in which GitHub, GitLab, or custom OIDC leak as conditions into the entire export path and CLI.

**Problem:**

Provider APIs differ and should not leak into the exporter.
The main concern in this area is separating what is common to ChangeTrace from what depends on a specific hosting provider and its API limits.

**Decision:**

Enrichment is implemented through provider-specific implementations behind a shared interface.
This decision pushes provider differences behind specialized contracts instead of spreading them through the export path and CLI.

**Rejected:**

- Jeden GitHub-centric enricher.
- Putting provider conditions directly in `RepositoryExporter`.
- Flattening all providers to the same lowest common denominator at the cost of losing their real constraints.
- Spreading provider-specific logic across the exporter, prompts, auth flows, and history-reading layers.

**Consequences:**

Adding providers becomes easier, but the enrichment resolver becomes an important contract.
This improves extensibility and makes behavior for new providers more predictable, but explicit fallbacks and unsupported scenarios still have to be maintained.

[ADR Home](../README.md) | [Category Index](./README.md) | [Next](021-make-enrichment-optional-per-export.md)
