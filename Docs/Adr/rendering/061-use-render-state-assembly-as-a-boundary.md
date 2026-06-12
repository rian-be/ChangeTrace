[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](060-use-deterministic-color-palettes.md) | [Next](066-buffer-and-aggregate-playback-events-before-scene-dispatch.md)

## [061] Use Render State Assembly As A Boundary

*2026-02* | Status: accepted

**Context:**

The graphics backend expects one coherent input object per frame, but the rendering layer builds that input from multiple sources: camera state, scene snapshots, HUD data, diagnostics, and selection state. Those sources should not be coupled directly to backend submission.

**Problem:**

If each output assembles frame state on its own, backend adapters end up re encoding scene contracts and HUD composition rules locally. That makes outputs harder to keep consistent and spreads frame shape decisions outside the rendering core.

**Decision:**

`RenderStateAssembler` builds an explicit render state as a dedicated boundary object. Rendering systems contribute source state, and backend outputs consume the assembled result instead of composing that frame contract themselves.

**Rejected:**

- Letting every output assemble state on its own.
- Treating the HUD and scene as unrelated data sources.
- Building visual behavior directly from the timeline model without a separate scene representation.
- Mixing frame assembly responsibilities into backend adapters or scene systems.

**Consequences:**

Outputs receive a consistent view of one frame contract, and backend code stays simpler. The tradeoff is that the assembler becomes an important integration point whose shape has to evolve carefully with both scene and HUD behavior.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](060-use-deterministic-color-palettes.md) | [Next](066-buffer-and-aggregate-playback-events-before-scene-dispatch.md)
