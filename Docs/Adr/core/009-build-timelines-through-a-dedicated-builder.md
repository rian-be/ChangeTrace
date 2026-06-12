[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](008-keep-unstable-query-apis-internal.md) | [Next](010-model-repository-activity-as-trace-events.md)

## [009] Build Timelines Through A Dedicated Builder

*2026-02* | Status: accepted

**Context:**

This decision concerns the language the system uses to describe the repository and its history. It is the layer used by export, the player, aggregation, and rendering, so its goal is not the convenience of one module but a shared and stable model for all of them.

**Problem:**

Converting Git history into a timeline requires consistent ordering of commits, branches, files, and statistics.
In practice, the goal is to prevent raw Git history from leaking into other layers as an unstructured set of strings, dates, and ad hoc structures.

**Decision:**

The timeline is built through a dedicated `TimelineBuilder`, not inside the Git reader or the renderer.
The responsibility boundary here runs between the domain model and infrastructure: export, providers, the player, and rendering consume the established model instead of defining it independently.

**Rejected:**

- Building the timeline separately in each consumer.
- Combining Git reading with event interpretation.
- Reinterpreting the domain model locally in each layer.
- A hybrid in which part of the meaning remains in Core and part in infrastructure or the presentation layer.

**Consequences:**

Timeline logic becomes testable and easier to optimize, but it also becomes a central change point.
This decision shapes contracts across many modules, so every change in Core requires attention to compatibility and the refactoring cost outside the core itself.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](008-keep-unstable-query-apis-internal.md) | [Next](010-model-repository-activity-as-trace-events.md)
