[ADR Home](../README.md) | [Category Index](./README.md) | [Next](051-translate-trace-events-into-render-commands.md)

## [050] Separate Rendering Core From Graphics Runtime

*2026-05* | Status: accepted

**Context:**

This group of decisions concerns the logical layer of visual representation: how events become commands, commands become scene state, and scene state becomes snapshots and layout. It is a separate architecture inside the system, not a detail of a single renderer.

**Problem:**

Scene and translator logic should not depend on OpenTK.
Rendering needs its own operational model, because the transition from timeline data to scene state is not a simple one-to-one mapping.

**Decision:**

Rendering core is separated from the Graphics/OpenTK runtime.
This decision maintains a separate layer of scene state, commands, snapshots, and layout, with a clear boundary between event analysis and visual representation.

**Rejected:**

- OpenTK typy w Core/Rendering.
- One layer combining semantics and GPU execution.
- Building visual behavior directly from the timeline model without a separate scene representation.
- Mixing translator, layout, and rendering-runtime responsibilities in one layer.

**Consequences:**

Rendering can be tested without the GPU, but the contracts between layers must be maintained.
The cost is a larger set of internal contracts, but the renderer stays modular and can evolve without breaking Core.

[ADR Home](../README.md) | [Category Index](./README.md) | [Next](051-translate-trace-events-into-render-commands.md)
