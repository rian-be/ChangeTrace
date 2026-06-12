[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](021-make-enrichment-optional-per-export.md) | [Next](023-treat-unavailable-enrichment-as-degraded-mode.md)

## [022] Skip Unavailable GitHub Pr Data Without Failing Export

*2026-06* | Status: accepted

**Context:**

This decision clarifies where the system recognizes and handles differences between hosting providers. The goal is to avoid a situation in which GitHub, GitLab, or custom OIDC leak as conditions into the entire export path and CLI.

**Problem:**

GitHub PR API can zwrocic 404, mimo ze history Git is poprawnie czytelna.
The main concern in this area is separating what is common to ChangeTrace from what depends on a specific hosting provider and its API limits.

**Decision:**

404 dla PR enrichment is traktowane jako brak opcjonalnych danych PR.
This decision pushes provider differences behind specialized contracts instead of spreading them through the export path and CLI.

**Rejected:**

- Failing the export because the PR sidecar is missing.
- Retrying the same missing-data case on every resume.
- Flattening all providers to the same lowest common denominator at the cost of losing their real constraints.
- Spreading provider-specific logic across the exporter, prompts, auth flows, and history-reading layers.

**Consequences:**

Repositories with unavailable PR data still produce `.gittrace`, and tests need to cover that case.
This improves extensibility and makes behavior for new providers more predictable, but explicit fallbacks and unsupported scenarios still have to be maintained.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](021-make-enrichment-optional-per-export.md) | [Next](023-treat-unavailable-enrichment-as-degraded-mode.md)
