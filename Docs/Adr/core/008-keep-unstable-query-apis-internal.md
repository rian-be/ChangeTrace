[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](007-use-result-types-for-recoverable-domain-operations.md) | [Next](009-build-timelines-through-a-dedicated-builder.md)

## [008] Keep Unstable Query Apis Internal

*2026-02* | Status: accepted

**Context:**

This decision concerns the language the system uses to describe the repository and its history. It is the layer used by export, the player, aggregation, and rendering, so its goal is not the convenience of one module but a shared and stable model for all of them.

**Problem:**

Early analytical queries changed quickly and should not freeze a public API too soon.
In practice, the goal is to prevent raw Git history from leaking into other layers as an unstructured set of strings, dates, and ad hoc structures.

**Decision:**

Immature specifications and helpers remain internal until their contracts are stable.
The responsibility boundary here runs between the domain model and infrastructure: export, providers, the player, and rendering consume the established model instead of defining it independently.

**Rejected:**

- A public API for every experimental rule.
- No dedicated query layer at all.
- Reinterpreting the domain model locally in each layer.
- A hybrid in which part of the meaning remains in Core and part in infrastructure or the presentation layer.

**Consequences:**

The project can refactor analytical queries without incurring public compatibility costs.
This decision shapes contracts across many modules, so every change in Core requires attention to compatibility and the refactoring cost outside the core itself.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](007-use-result-types-for-recoverable-domain-operations.md) | [Next](009-build-timelines-through-a-dedicated-builder.md)
