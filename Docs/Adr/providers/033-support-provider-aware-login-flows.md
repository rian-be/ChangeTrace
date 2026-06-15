[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](023-treat-unavailable-enrichment-as-degraded-mode.md) | [Next](053-use-provider-detection-from-repository-urls.md)

## [033] Support Provider Aware Login Flows

*2026-06* | Status: accepted

**Context:**

Different hosting providers expose different authentication capabilities: device flow, browser redirect with PKCE, custom OIDC discovery, or no support for a given flow at all. The CLI cannot treat login as one uniform exchange while still supporting those hosts cleanly.

**Problem:**

If login is modeled as one global flow, the system either overfits to one provider or degrades every integration to the weakest common behavior. That makes auth handling brittle and pushes provider capability checks into CLI commands and session setup.

**Decision:**

Login chooses a provider specific flow instead of one global scheme. Provider selection determines which auth contract is used, and the resulting session still lands in the shared local auth model.

**Rejected:**

- One GitHub device flow implementation for every provider.
- Token only auth for providers without device flow.
- Flattening all providers to the same lowest common denominator at the cost of losing their real constraints.
- Spreading provider specific logic across the exporter, prompts, auth flows, and history reading layers.

**Consequences:**

Auth support becomes more flexible and host integrations can use the flows they actually support. The cost is that capability detection and prompt behavior must stay aligned with provider specific auth implementations.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](023-treat-unavailable-enrichment-as-degraded-mode.md) | [Next](053-use-provider-detection-from-repository-urls.md)
