[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](053-use-provider-detection-from-repository-urls.md) | [Next](094-use-device-flow-where-providers-support-it.md)

## [093] Resolve Timeline Enrichers By Provider

*2026-06* | Status: accepted

**Context:**

Once provider identity is known, the export pipeline still has to choose the correct enrichment implementation for the current host and selected export scope. That choice belongs in one resolver instead of in exporter conditionals.

**Problem:**

The exporter needs to resolve the correct enricher from provider identity and active call configuration. Without a dedicated resolution point, host selection logic leaks into export stages and makes unsupported provider behavior harder to centralize.

**Decision:**

Enricher selection goes through a provider aware resolver. The exporter asks for the matching enrichment implementation and then executes a host specific path without encoding provider branches locally.

**Rejected:**

- Manual `if/else` branching in the export path.
- Providing no enrichment for providers other than GitHub.
- Resolving enrichers ad hoc inside CLI handlers before export starts.
- Hiding unsupported provider behavior behind null returns with no explicit policy.

**Consequences:**

Export stays extensible and provider support becomes easier to add in one place. The tradeoff is that the resolver becomes a key contract for unsupported cases, fallback policy, and host specific scope handling.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](053-use-provider-detection-from-repository-urls.md) | [Next](094-use-device-flow-where-providers-support-it.md)
