[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](010-model-repository-activity-as-trace-events.md) | [Next](065-use-a-reusable-aggregation-engine-for-streaming-semantic-processing.md)

## [011] Optimize Timeline Builder Hot Paths

*2026-06* | Status: accepted

**Context:**

This decision concerns the language the system uses to describe the repository and its history. It is the layer used by export, the player, aggregation, and rendering, so its goal is not the convenience of one module but a shared and stable model for all of them.

**Problem:**

Timeline building runs for every export and can dominate the cost of large repositories.
In practice, the goal is to prevent raw Git history from leaking into other layers as an unstructured set of strings, dates, and ad hoc structures.

**Decision:**

`TimelineBuilder` hot paths are optimized and measured.
The responsibility boundary here runs between the domain model and infrastructure: export, providers, the player, and rendering consume the established model instead of defining it independently.

**Rejected:**

- Accepting a slow builder as an unavoidable domain cost.
- Micro optimizations without measurement.
- Reinterpreting the domain model locally in each layer.
- A hybrid in which part of the meaning remains in Core and part in infrastructure or the presentation layer.

**Consequences:**

Export becomes faster, but hot path code can be less straightforward and requires benchmarks.
This decision shapes contracts across many modules, so every change in Core requires attention to compatibility and the refactoring cost outside the core itself.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](010-model-repository-activity-as-trace-events.md) | [Next](065-use-a-reusable-aggregation-engine-for-streaming-semantic-processing.md)
