[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](057-maintain-a-scene-graph-for-visualization-state.md) | [Next](059-support-manual-camera-control.md)

## [058] Render From Immutable Scene Snapshots

*2026-05* | Status: accepted

**Context:**

Scene update and scene rendering do not happen for the same reasons and should not compete over mutable state. The backend needs a stable frame view while layout, animation, and hover systems continue to manage the live scene model.

**Problem:**

If backend rendering reads directly from the mutable scene graph, update order and timing become part of rendering correctness. That creates risks around partially updated state, frame to frame inconsistency, and accidental coupling between backend rendering and scene mutation rules.

**Decision:**

Backend rendering operates on immutable scene snapshots derived from the live scene graph. Snapshot assembly produces a stable render time view that the graphics layer can consume without mutating or observing the scene graph in flight.

**Rejected:**

- Reading the mutable scene graph directly.
- Blocking scene updates during rendering.
- Building visual behavior directly from the timeline model without a separate scene representation.
- Treating snapshot assembly as an optional optimization instead of a rendering boundary.

**Consequences:**

Frame submission sees a consistent scene image and backend code stays insulated from live mutation behavior. The tradeoff is extra snapshot assembly work and another contract that must stay aligned with scene state.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](057-maintain-a-scene-graph-for-visualization-state.md) | [Next](059-support-manual-camera-control.md)
