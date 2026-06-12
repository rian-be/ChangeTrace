[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](066-buffer-and-aggregate-playback-events-before-scene-dispatch.md) | [Next](070-represent-scene-relations-explicitly.md)

## [067] Route Renderable Semantic Events Through An Explicit Dispatch Table

*2026-06* | Status: accepted

**Context:**

After aggregation flush, `RenderingPipeline` does not keep separate branches for every event type scattered across the code. `RenderEventDispatchTable` maintains an explicit `RenderEventKinds -> DispatchFn` map that connects renderable categories with the appropriate semantic writer.

**Problem:**

As the number of semantic event types grows, manual branching in the pipeline quickly becomes a source of accidental mismatches between `RenderEventKinds` flags, writers, and dispatch calls. Rendering needs one place where the full routing of supported event classes is visible.

**Decision:**

Routing semantic events into the scene pipeline is maintained in an explicit dispatch table. Rendering treats renderable types as a declarative map rather than a set of hidden conditions in many classes.

**Rejected:**

- Scattering `switch` or `if` branches throughout `RenderingPipeline`.
- Resolving handlers by reflection on every flush.
- Coupling `RenderEventKinds` configuration directly to specific translators.
- Hiding routing inside aggregators instead of maintaining an explicit dispatch boundary.

**Consequences:**

Adding a new render event category requires one explicit entry and keeps routing in a single place. The drawback is that the dispatch table becomes a central compatibility contract between aggregation, translation, and the rest of the scene pipeline.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](066-buffer-and-aggregate-playback-events-before-scene-dispatch.md) | [Next](070-represent-scene-relations-explicitly.md)
