[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](093-resolve-timeline-enrichers-by-provider.md) | [Next](095-support-custom-oidc-providers-through-dynamic-discovery-and-local-callbacks.md)

## [094] Use Device Flow Where Providers Support It

*2026-06* | Status: accepted

**Context:**

This decision clarifies where the system recognizes and handles differences between hosting providers. The goal is to avoid a situation in which GitHub, GitLab, or custom OIDC leak as conditions into the entire export path and CLI.

**Problem:**

Device flow fits a CLI well and does not require a local callback server.
The main concern in this area is separating what is common to ChangeTrace from what depends on a specific hosting provider and its API limits.

**Decision:**

GitHub and GitLab use device flow.
This decision pushes provider differences behind specialized contracts instead of spreading them through the export path and CLI.

**Rejected:**

- Wklejanie personal access tokenow jako podstawowy tryb.
- Browser callback jako jedyny sposob logowania.
- Flattening all providers to the same lowest common denominator at the cost of losing their real constraints.
- Spreading provider-specific logic across the exporter, prompts, auth flows, and history-reading layers.

**Consequences:**

Login CLI is prostszy, but provider must wspierac ten flow.
This improves extensibility and makes behavior for new providers more predictable, but explicit fallbacks and unsupported scenarios still have to be maintained.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](093-resolve-timeline-enrichers-by-provider.md) | [Next](095-support-custom-oidc-providers-through-dynamic-discovery-and-local-callbacks.md)
