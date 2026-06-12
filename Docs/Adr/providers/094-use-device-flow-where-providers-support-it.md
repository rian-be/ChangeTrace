[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](093-resolve-timeline-enrichers-by-provider.md) | [Next](095-support-custom-oidc-providers-through-dynamic-discovery-and-local-callbacks.md)

## [094] Use Device Flow Where Providers Support It

*2026-06* | Status: accepted

**Context:**

GitHub and GitLab offer device flow that fits a CLI well because it avoids local callback plumbing and still preserves a standards based authorization path. For a local first command line application, that tradeoff is often better than forcing a browser redirect listener when the provider does not require it.

**Problem:**

CLI login should avoid unnecessary local callback infrastructure when a provider already supports a safer out of band authorization mechanism. For providers with device flow, using a more complex flow adds operational friction without buying better integration.

**Decision:**

GitHub and GitLab use device flow. The provider layer selects that auth path when it is supported, while other providers can still use different flows through the same auth architecture.

**Rejected:**

- Making browser callback flow the default for every provider.
- Using pasted personal access tokens as the primary login mode.
- Treating device flow as a custom CLI shortcut outside the common auth abstractions.
- Pretending all providers support the same login mechanism.

**Consequences:**

CLI login becomes simpler for supported providers and avoids running local callback infrastructure unnecessarily. The tradeoff is that auth behavior now depends on provider capability, which has to remain visible in prompts, docs, and tests.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](093-resolve-timeline-enrichers-by-provider.md) | [Next](095-support-custom-oidc-providers-through-dynamic-discovery-and-local-callbacks.md)
