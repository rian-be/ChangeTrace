[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](095-support-custom-oidc-providers-through-dynamic-discovery-and-local-callbacks.md)

## [096] Use Authorization Code PKCE For OIDC Providers

*2026-06* | Status: accepted

**Context:**

Not every provider offers device flow, and custom OIDC integrations need a standards based browser driven authorization path. Codeberg and dynamic OIDC providers fit that model better than token paste or device code behavior.

**Problem:**

For providers without device flow, the CLI still needs a secure user authorization path that works with public clients and does not rely on long lived static secrets. Falling back to passwords or pasted long lived tokens would weaken the auth model and fragment session semantics.

**Decision:**

Codeberg and custom providers use authorization code flow with PKCE. The provider layer runs that flow through local callback handling and feeds the resulting session back into the shared auth model.

**Rejected:**

- Having no support for providers that do not offer device flow.
- Using passwords or long lived tokens as a fallback.
- Flattening all providers to the same lowest common denominator at the cost of losing their real constraints.
- Treating OIDC redirect flow as a provider specific special case outside shared auth contracts.

**Consequences:**

The provider set can expand beyond device flow hosts while keeping a modern auth posture. The cost is additional callback, PKCE, and OIDC configuration complexity that has to remain validated and observable.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](095-support-custom-oidc-providers-through-dynamic-discovery-and-local-callbacks.md)
