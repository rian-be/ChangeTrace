[ADR Home](../README.md)

# providers

This category covers hosting-provider-dependent behavior such as detection, login, enrichment, and fallbacks.

## Scope

Look here for provider detection, provider-aware auth, enrichment routing, and host-specific capability differences.

## Responsibility Boundaries

Providers should not define the timeline model or the `.gittrace` format. This layer isolates host variability from the rest of the system.

## How To Start Reading

Start here for any change that depends on GitHub, GitLab, Codeberg, or custom OIDC behavior.

## ADR List

| ADR | Title |
| --- | --- |
| [020-keep-provider-specific-enrichment-behind-interfaces.md](./020-keep-provider-specific-enrichment-behind-interfaces.md) | Keep Provider Specific Enrichment Behind Interfaces |
| [021-make-enrichment-optional-per-export.md](./021-make-enrichment-optional-per-export.md) | Make Enrichment Optional Per Export |
| [022-skip-unavailable-github-pr-data-without-failing-export.md](./022-skip-unavailable-github-pr-data-without-failing-export.md) | Skip Unavailable GitHub Pr Data Without Failing Export |
| [023-treat-unavailable-enrichment-as-degraded-mode.md](./023-treat-unavailable-enrichment-as-degraded-mode.md) | Treat Unavailable Enrichment As Degraded Mode |
| [033-support-provider-aware-login-flows.md](./033-support-provider-aware-login-flows.md) | Support Provider Aware Login Flows |
| [053-use-provider-detection-from-repository-urls.md](./053-use-provider-detection-from-repository-urls.md) | Use Provider Detection From Repository Urls |
| [093-resolve-timeline-enrichers-by-provider.md](./093-resolve-timeline-enrichers-by-provider.md) | Resolve Timeline Enrichers By Provider |
| [094-use-device-flow-where-providers-support-it.md](./094-use-device-flow-where-providers-support-it.md) | Use Device Flow Where Providers Support It |
| [095-support-custom-oidc-providers-through-dynamic-discovery-and-local-callbacks.md](./095-support-custom-oidc-providers-through-dynamic-discovery-and-local-callbacks.md) | Support Custom OIDC Providers Through Dynamic Discovery And Local Callbacks |
| [096-use-authorization-code-pkce-for-oidc-providers.md](./096-use-authorization-code-pkce-for-oidc-providers.md) | Use Authorization Code PKCE For OIDC Providers |
