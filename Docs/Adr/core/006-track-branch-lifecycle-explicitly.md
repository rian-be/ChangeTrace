[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](005-represent-identifiers-as-value-objects.md) | [Next](007-use-result-types-for-recoverable-domain-operations.md)

## [006] Track Branch Lifecycle Explicitly

*2026-02* | Status: accepted

**Context:**

This decision concerns the language the system uses to describe the repository and its history. It is the layer used by export, the player, aggregation, and rendering, so its goal is not the convenience of one module but a shared and stable model for all of them.

**Problem:**

Branches change state over time and cannot be visualized correctly from individual commits alone.
In practice, the goal is to prevent raw Git history from leaking into other layers as an unstructured set of strings, dates, and ad hoc structures.

**Decision:**

Branch lifecycle is maintained through dedicated tracking logic.
The responsibility boundary here runs between the domain model and infrastructure: export, providers, the player, and rendering consume the established model instead of defining it independently.

**Rejected:**

- Treating branches as simple labels.
- Reconstructing branch state only in the renderer.
- Reinterpreting the domain model locally in each layer.
- A hybrid in which part of the meaning remains in Core and part in infrastructure or the presentation layer.

**Consequences:**

The timeline carries richer branch context, but correct branch tracking requires dedicated test coverage.
This decision shapes contracts across many modules, so every change in Core requires attention to compatibility and the refactoring cost outside the core itself.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](005-represent-identifiers-as-value-objects.md) | [Next](007-use-result-types-for-recoverable-domain-operations.md)
