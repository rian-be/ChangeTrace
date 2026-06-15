[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](003-keep-filtering-behind-specifications.md) | [Next](005-represent-identifiers-as-value-objects.md)

## [004] Keep The Domain Model Independent From Git Providers

*2026-02* | Status: accepted

**Context:**

This decision concerns the language the system uses to describe the repository and its history. It is the layer used by export, the player, aggregation, and rendering, so its goal is not the convenience of one module but a shared and stable model for all of them.

**Problem:**

Repository history comes from Git, but ChangeTrace analysis should not depend on GitHub, GitLab, or Codeberg.
In practice, the goal is to prevent raw Git history from leaking into other layers as an unstructured set of strings, dates, and ad hoc structures.

**Decision:**

Core describes the repository through its own domain types, events, timeline structures, and value objects, without dependencies on provider APIs.
The responsibility boundary here runs between the domain model and infrastructure: export, providers, the player, and rendering consume the established model instead of defining it independently.

**Rejected:**

- A domain model built directly on provider responses.
- Using raw Git records as the application's public model.
- Reinterpreting the domain model locally in each layer.
- A hybrid in which part of the meaning remains in Core and part in infrastructure or the presentation layer.

**Consequences:**

Core can be used by export, the player, and rendering, while provider specific data is isolated in enrichment or sidecars.
This decision shapes contracts across many modules, so every change in Core requires attention to compatibility and the refactoring cost outside the core itself.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](003-keep-filtering-behind-specifications.md) | [Next](005-represent-identifiers-as-value-objects.md)
