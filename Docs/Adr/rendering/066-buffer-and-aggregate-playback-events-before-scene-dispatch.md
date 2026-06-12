[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](061-use-render-state-assembly-as-a-boundary.md) | [Next](067-route-renderable-semantic-events-through-an-explicit-dispatch-table.md)

## [066] Buffer And Aggregate Playback Events Before Scene Dispatch

*2026-06* | Status: accepted

**Context:**

`RenderingPipeline` does not dispatch raw `TraceEvent` objects directly into the scene. Events coming from the player go into `RenderEventBuffer`, pass through `TraceEventAggregationStage`, and only after flush become semantic input for translators and scene dispatchers.

**Problem:**

Rendering needs to work on semantic groups and configurable event types, not on every raw event as soon as it arrives. Without a buffer and aggregation stage, scene logic would have to understand low level events, bundling semantics, and enabling or disabling render event categories all at once.

**Decision:**

Playback events are first buffered and aggregated inside the rendering layer, and the scene only sees the resulting semantic stream. Changing active `RenderEventKinds` rebuilds aggregation state instead of scattering filtering conditions across translators and scene handlers.

**Rejected:**

- Dispatching raw `TraceEvent` directly to `SceneCommandDispatcher`.
- Aggregating only on the side of the scene graph or renderers.
- Mixing `RenderEventKinds` filtering with translator logic.
- One shot event processing with no ability to flush or reset aggregation state.

**Consequences:**

Rendering receives higher level semantic inputs and can control which event categories participate in scene construction at all. The cost is extra pipeline state that must be reset correctly when render event modes change or playback is cleared.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](061-use-render-state-assembly-as-a-boundary.md) | [Next](067-route-renderable-semantic-events-through-an-explicit-dispatch-table.md)
