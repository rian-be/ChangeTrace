[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](058-render-from-immutable-scene-snapshots.md) | [Next](060-use-deterministic-color-palettes.md)

## [059] Support Manual Camera Control

*2026-05* | Status: accepted

**Context:**

This group of decisions concerns the logical layer of visual representation: how events become commands, commands become scene state, and scene state becomes snapshots and layout. It is a separate architecture inside the system, not a detail of a single renderer.

**Problem:**

Automatic tracking is not enough to inspect complex scenes.
Rendering needs its own operational model, because the transition from timeline data to scene state is not a simple one-to-one mapping.

**Decision:**

The renderer exposes manual camera control alongside tracking modes.
This decision maintains a separate layer of scene state, commands, snapshots, and layout, with a clear boundary between event analysis and visual representation.

**Rejected:**

- Only an automatic camera.
- Managing the camera outside the input/rendering model.
- Building visual behavior directly from the timeline model without a separate scene representation.
- Mixing translator, layout, and rendering-runtime responsibilities in one layer.

**Consequences:**

The operator can explore the scene, but input and camera must stay synchronized.
The cost is a larger set of internal contracts, but the renderer stays modular and can evolve without breaking Core.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](058-render-from-immutable-scene-snapshots.md) | [Next](060-use-deterministic-color-palettes.md)
