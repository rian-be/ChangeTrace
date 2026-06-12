[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](006-track-branch-lifecycle-explicitly.md) | [Next](008-keep-unstable-query-apis-internal.md)

## [007] Use Result Types For Recoverable Domain Operations

*2026-02* | Status: accepted

**Context:**

This decision concerns the language the system uses to describe the repository and its history. It is the layer used by export, the player, aggregation, and rendering, so its goal is not the convenience of one module but a shared and stable model for all of them.

**Problem:**

Some domain operations can fail in predictable ways and should not be modeled only with exceptions.
In practice, the goal is to prevent raw Git history from leaking into other layers as an unstructured set of strings, dates, and ad hoc structures.

**Decision:**

Controlled failures use `Result` and `Result<T>` types.
The responsibility boundary here runs between the domain model and infrastructure: export, providers, the player, and rendering consume the established model instead of defining it independently.

**Rejected:**

- Using exceptions as the only error-flow mechanism.
- Returning nulls or magic values.
- Reinterpreting the domain model locally in each layer.
- A hybrid in which part of the meaning remains in Core and part in infrastructure or the presentation layer.

**Consequences:**

Domain failures become more explicit, but callers must handle the result instead of assuming success.
This decision shapes contracts across many modules, so every change in Core requires attention to compatibility and the refactoring cost outside the core itself.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](006-track-branch-lifecycle-explicitly.md) | [Next](008-keep-unstable-query-apis-internal.md)
