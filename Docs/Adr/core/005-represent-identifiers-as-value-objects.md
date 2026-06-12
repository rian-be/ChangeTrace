[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](004-keep-the-domain-model-independent-from-git-providers.md) | [Next](006-track-branch-lifecycle-explicitly.md)

## [005] Represent Identifiers As Value Objects

*2026-02* | Status: accepted

**Context:**

This decision concerns the language the system uses to describe the repository and its history. It is the layer used by export, the player, aggregation, and rendering, so its goal is not the convenience of one module but a shared and stable model for all of them.

**Problem:**

Actor names, file paths, SHAs, branches, and repositories are semantically different even though they are often represented as strings.
In practice, the goal is to prevent raw Git history from leaking into other layers as an unstructured set of strings, dates, and ad hoc structures.

**Decision:**

Domain identifiers are represented by explicit value types instead of primitives.
The responsibility boundary here runs between the domain model and infrastructure: export, providers, the player, and rendering consume the established model instead of defining it independently.

**Rejected:**

- Passing raw strings through every layer.
- Validating meaning only at the usage sites.
- Reinterpreting the domain model locally in each layer.
- A hybrid in which part of the meaning remains in Core and part in infrastructure or the presentation layer.

**Consequences:**

The code becomes more self documenting and type safe, but serialization and tests must handle these types explicitly.
This decision shapes contracts across many modules, so every change in Core requires attention to compatibility and the refactoring cost outside the core itself.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](004-keep-the-domain-model-independent-from-git-providers.md) | [Next](006-track-branch-lifecycle-explicitly.md)
