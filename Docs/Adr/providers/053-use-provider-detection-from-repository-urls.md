[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](033-support-provider-aware-login-flows.md) | [Next](093-resolve-timeline-enrichers-by-provider.md)

## [053] Use Provider Detection From Repository Urls

*2026-02* | Status: accepted

**Context:**

The export and auth layers both need to know which hosting rules apply before they can choose enrichers, prompts, or login flows. Repository URLs already encode that information for the common cases, so the system has a natural signal for provider selection.

**Problem:**

If the provider has to be passed manually everywhere, repository driven workflows become noisy and easy to misconfigure. If the system assumes one default host, provider aware auth and enrichment become wrong as soon as the repository comes from another platform.

**Decision:**

The provider is resolved from the repository URL through a shared helper. That provider identity becomes the common input for auth selection, enrichment resolution, and related host specific behavior.

**Rejected:**

- Passing the provider manually in every situation.
- Assuming GitHub for every URL.
- Keeping separate provider detection logic in auth, export, and CLI layers.
- Falling back to provider specific heuristics scattered through repository processing code.

**Consequences:**

Provider aware auth and enrichment share one entry point, and repository driven workflows become easier to automate. The tradeoff is that URL parsing rules become a central compatibility surface that needs explicit tests for host variants and edge cases.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](033-support-provider-aware-login-flows.md) | [Next](093-resolve-timeline-enrichers-by-provider.md)
