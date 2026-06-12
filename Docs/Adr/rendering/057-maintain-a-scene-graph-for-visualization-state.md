[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](056-keep-animation-as-a-subsystem.md) | [Next](058-render-from-immutable-scene-snapshots.md)

## [057] Maintain A Scene Graph For Visualization State

*2026-02* | Status: accepted

**Context:**

This group of decisions concerns the logical layer of visual representation: how events become commands, commands become scene state, and scene state becomes snapshots and layout. It is a separate architecture inside the system, not a detail of a single renderer.

**Problem:**

Repository visualization has objects and relationships that persist across many frames.
Rendering needs its own operational model, because the transition from timeline data to scene state is not a simple one-to-one mapping.

**Decision:**

Scene state is maintained in a scene graph.
This decision maintains a separate layer of scene state, commands, snapshots, and layout, with a clear boundary between event analysis and visual representation.

**Rejected:**

- Rebuilding the scene from scratch for every event.
- Storing visual state inside the player.
- Building visual behavior directly from the timeline model without a separate scene representation.
- Mixing translator, layout, and rendering-runtime responsibilities in one layer.

**Consequences:**

The renderer can update state incrementally, but the scene graph requires lifecycle management for nodes.
The cost is a larger set of internal contracts, but the renderer stays modular and can evolve without breaking Core.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](056-keep-animation-as-a-subsystem.md) | [Next](058-render-from-immutable-scene-snapshots.md)
