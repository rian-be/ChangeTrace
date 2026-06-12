[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](060-use-deterministic-color-palettes.md) | [Next](066-buffer-and-aggregate-playback-events-before-scene-dispatch.md)

## [061] Use Render State Assembly As A Boundary

*2026-02* | Status: accepted

**Context:**

This group of decisions concerns the logical layer of visual representation: how events become commands, commands become scene state, and scene state becomes snapshots and layout. It is a separate architecture inside the system, not a detail of a single renderer.

**Problem:**

HUD, camera, scene, and statistics must be assembled into one renderable object.
Rendering needs its own operational model, because the transition from timeline data to scene state is not a simple one-to-one mapping.

**Decision:**

`RenderStateAssembler` builds an explicit render state.
This decision maintains a separate layer of scene state, commands, snapshots, and layout, with a clear boundary between event analysis and visual representation.

**Rejected:**

- Letting every output assemble state on its own.
- Treating the HUD and scene as unrelated data sources.
- Building visual behavior directly from the timeline model without a separate scene representation.
- Mixing translator, layout, and rendering-runtime responsibilities in one layer.

**Consequences:**

Outputs receive a consistent view, but the assembler becomes an important integration point.
The cost is a larger set of internal contracts, but the renderer stays modular and can evolve without breaking Core.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](060-use-deterministic-color-palettes.md) | [Next](066-buffer-and-aggregate-playback-events-before-scene-dispatch.md)
