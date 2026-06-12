[ADR Home](../README.md) | [Category Index](./README.md) | [Next](004-keep-the-domain-model-independent-from-git-providers.md)

## [003] Keep Filtering Behind Specifications

*2026-02* | Status: accepted

**Context:**

This decision concerns the language the system uses to describe the repository and its history. It is the layer used by export, the player, aggregation, and rendering, so its goal is not the convenience of one module but a shared and stable model for all of them.

**Problem:**

Timeline filtering appears in many places and is easy to duplicate.
In practice, the goal is to prevent raw Git history from leaking into other layers as an unstructured set of strings, dates, and ad hoc structures.

**Decision:**

Filtering rules are modeled as specifications and domain queries.
The responsibility boundary here runs between the domain model and infrastructure: export, providers, the player, and rendering consume the established model instead of defining it independently.

**Rejected:**

- Duplicating LINQ predicates in handlers.
- Publicly persisting experimental filters.
- Reinterpreting the domain model locally in each layer.
- A hybrid in which part of the meaning remains in Core and part in infrastructure or the presentation layer.

**Consequences:**

Filters are composable, but they must keep up with changes in the event model.
This decision shapes contracts across many modules, so every change in Core requires attention to compatibility and the refactoring cost outside the core itself.

[ADR Home](../README.md) | [Category Index](./README.md) | [Next](004-keep-the-domain-model-independent-from-git-providers.md)
