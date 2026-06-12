[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](050-separate-rendering-core-from-graphics-runtime.md) | [Next](052-use-render-commands-as-visual-intent.md)

## [051] Translate Trace Events Into Render Commands

*2026-02* | Status: accepted

**Context:**

This group of decisions concerns the logical layer of visual representation: how events become commands, commands become scene state, and scene state becomes snapshots and layout. It is a separate architecture inside the system, not a detail of a single renderer.

**Problem:**

The timeline model should not also be the renderer API.
Rendering needs its own operational model, because the transition from timeline data to scene state is not a simple one-to-one mapping.

**Decision:**

Rendering uses translators that convert events into render commands.
This decision maintains a separate layer of scene state, commands, snapshots, and layout, with a clear boundary between event analysis and visual representation.

**Rejected:**

- Renderer czyta TraceEvent directly.
- Pola wizualne w modelu Core.
- Building visual behavior directly from the timeline model without a separate scene representation.
- Mixing translator, layout, and rendering-runtime responsibilities in one layer.

**Consequences:**

The Core/Rendering boundary is cleaner, but translators must track event semantics carefully.
The cost is a larger set of internal contracts, but the renderer stays modular and can evolve without breaking Core.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](050-separate-rendering-core-from-graphics-runtime.md) | [Next](052-use-render-commands-as-visual-intent.md)
