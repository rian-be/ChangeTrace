[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](020-keep-provider-specific-enrichment-behind-interfaces.md) | [Next](022-skip-unavailable-github-pr-data-without-failing-export.md)

## [021] Make Enrichment Optional Per Export

*2026-06* | Status: accepted

**Context:**

This decision clarifies where the system recognizes and handles differences between hosting providers. The goal is to avoid a situation in which GitHub, GitLab, or custom OIDC leak as conditions into the entire export path and CLI.

**Problem:**

Provider APIs can be slow, unavailable, or unnecessary for a given export.
The main concern in this area is separating what is common to ChangeTrace from what depends on a specific hosting provider and its API limits.

**Decision:**

Export exposes explicit options and prompts that control enrichment.
This decision pushes provider differences behind specialized contracts instead of spreading them through the export path and CLI.

**Rejected:**

- Always attempting full enrichment.
- Hiding enrichment from the calling layer.
- Flattening all providers to the same lowest common denominator at the cost of losing their real constraints.
- Spreading provider-specific logic across the exporter, prompts, auth flows, and history-reading layers.

**Consequences:**

The operator controls export cost and risk, but the CLI must stay consistent across interactive and automated modes.
This improves extensibility and makes behavior for new providers more predictable, but explicit fallbacks and unsupported scenarios still have to be maintained.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](020-keep-provider-specific-enrichment-behind-interfaces.md) | [Next](022-skip-unavailable-github-pr-data-without-failing-export.md)
