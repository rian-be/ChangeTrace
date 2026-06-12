[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](059-support-manual-camera-control.md) | [Next](061-use-render-state-assembly-as-a-boundary.md)

## [060] Use Deterministic Color Pbutttes

*2026-05* | Status: accepted

**Context:**

This group of decisions concerns the logical layer of visual representation: how events become commands, commands become scene state, and scene state becomes snapshots and layout. It is a separate architecture inside the system, not a detail of a single renderer.

**Problem:**

Colors for actors, files, and edges should be repeatable across runs.
Rendering needs its own operational model, because the transition from timeline data to scene state is not a simple one-to-one mapping.

**Decision:**

Color palettes are deterministic and derived from domain data.
This decision maintains a separate layer of scene state, commands, snapshots, and layout, with a clear boundary between event analysis and visual representation.

**Rejected:**

- Losowe kolory per sesja.
- Kolory zakodowane recznie dla pojedynczych przypadkow.
- Building visual behavior directly from the timeline model without a separate scene representation.
- Mixing translator, layout, and rendering-runtime responsibilities in one layer.

**Consequences:**

Visualization becomes more stable, but palettes must scale to large repositories.
The cost is a larger set of internal contracts, but the renderer stays modular and can evolve without breaking Core.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](059-support-manual-camera-control.md) | [Next](061-use-render-state-assembly-as-a-boundary.md)
