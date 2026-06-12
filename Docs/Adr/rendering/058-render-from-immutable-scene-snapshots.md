[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](057-maintain-a-scene-graph-for-visualization-state.md) | [Next](059-support-manual-camera-control.md)

## [058] Render From Immutable Scene Snapshots

*2026-02* | Status: accepted

**Context:**

This group of decisions concerns the logical layer of visual representation: how events become commands, commands become scene state, and scene state becomes snapshots and layout. It is a separate architecture inside the system, not a detail of a single renderer.

**Problem:**

The rendering backend should read a stable scene image rather than mutable state during updates.
Rendering needs its own operational model, because the transition from timeline data to scene state is not a simple one-to-one mapping.

**Decision:**

Rendering operates from scene snapshots.
This decision maintains a separate layer of scene state, commands, snapshots, and layout, with a clear boundary between event analysis and visual representation.

**Rejected:**

- Reading the mutable scene graph directly.
- Blocking scene updates during rendering.
- Building visual behavior directly from the timeline model without a separate scene representation.
- Mixing translator, layout, and rendering-runtime responsibilities in one layer.

**Consequences:**

Granica frame update/render is jasna, but snapshots can generowac cost alokacji.
The cost is a larger set of internal contracts, but the renderer stays modular and can evolve without breaking Core.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](057-maintain-a-scene-graph-for-visualization-state.md) | [Next](059-support-manual-camera-control.md)
