[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](094-use-device-flow-where-providers-support-it.md) | [Next](096-use-authorization-code-pkce-for-oidc-providers.md)

## [095] Support Custom OIDC Providers Through Dynamic Discovery And Local Callbacks

*2026-06* | Status: accepted

**Context:**

The provider layer is not limited to built in integrations such as GitHub or GitLab. `CustomOAuthProvider` introduces a `custom` model based on OIDC discovery, a local HTTP callback, PKCE, and a named `provider key`, allowing auth to work with non standard issuers without hardcoding them into the codebase.

**Problem:**

The system has to support a provider that is not known at compile time while still fitting into the same auth session model and provider aware workflow. Without an explicit dynamic discovery decision, auth would tend to hard code a few integrations or split custom login away from the rest of the provider contracts.

**Decision:**

ChangeTrace supports custom OIDC providers through dynamic discovery document retrieval, a local callback listener, and a named session key of `custom:<slug>`. A custom provider stays inside the same auth architecture as the built in integrations, but with a different input configuration model.

**Rejected:**

- Supporting only a fixed list of built in providers.
- Keeping custom provider endpoints as fixed code configuration without discovery.
- Requiring an external callback service instead of a local redirect listener.
- Splitting custom OIDC into a separate auth path without a shared `IAuthProvider` contract.

**Consequences:**

The system becomes more flexible for new issuers and self hosted installations, and custom auth does not require a new integration per provider. The tradeoff is a more complex model for input validation, local callbacks, and dynamically named sessions.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](094-use-device-flow-where-providers-support-it.md) | [Next](096-use-authorization-code-pkce-for-oidc-providers.md)
