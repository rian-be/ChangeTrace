[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](033-support-provider-aware-login-flows.md) | [Next](093-resolve-timeline-enrichers-by-provider.md)

## [053] Use Provider Detection From Repository Urls

*2026-02* | Status: accepted

**Context:**

This decision clarifies where the system recognizes and handles differences between hosting providers. The goal is to avoid a situation in which GitHub, GitLab, or custom OIDC leak as conditions into the entire export path and CLI.

**Problem:**

Export initiated from a URL should know whether the repository comes from GitHub, GitLab, Codeberg, or another host.
The main concern in this area is separating what is common to ChangeTrace from what depends on a specific hosting provider and its API limits.

**Decision:**

The provider is resolved from the repository URL through a shared helper.
This decision pushes provider differences behind specialized contracts instead of spreading them through the export path and CLI.

**Rejected:**

- Passing the provider manually in every situation.
- Assuming GitHub for every URL.
- Flattening all providers to the same lowest common denominator at the cost of losing their real constraints.
- Spreading provider-specific logic across the exporter, prompts, auth flows, and history-reading layers.

**Consequences:**

Provider-aware auth and enrichment share one entry point, but the URL parser must be tested carefully.
This improves extensibility and makes behavior for new providers more predictable, but explicit fallbacks and unsupported scenarios still have to be maintained.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](033-support-provider-aware-login-flows.md) | [Next](093-resolve-timeline-enrichers-by-provider.md)
