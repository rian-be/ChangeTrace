[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](073-advance-scene-systems-through-a-dedicated-frame-updater.md) | [Next](075-resolve-hover-state-through-a-separate-scene-query-service.md)

## [074] Assemble And Submit Render Frames Through A Thin Boundary

*2026-06* | Status: accepted

**Context:**

`RenderFrameAssembler` is a thin boundary between rendering logic and the output backend. It gathers the dependencies required to build `RenderState` through `IRenderStateAssembler` and then passes the finished snapshot to `IRenderOutput`.

**Problem:**

Render state assembly and frame delivery to the backend are two different contracts. When the pipeline both builds state and drives the output directly, the boundary between logical frame representation and graphics backend execution becomes harder to preserve.

**Decision:**

Frame assembly and submission are kept behind a thin frame assembler that connects `IRenderStateAssembler` with `IRenderOutput`. `RenderingPipeline` does not talk to the output at the level of state details; it only passes the parameters needed to build one immutable frame.

**Rejected:**

- Direct `assembler.Assemble(...); output.Submit(...)` calls scattered through the pipeline.
- Exposing a mutable scene graph directly to the output backend.
- Combining HUD assembly, snapshots, and submission with the frame updater.
- Moving responsibility for `RenderState` construction into the `graphics/` backend.

**Consequences:**

The `rendering -> output` boundary remains clear, and `RenderState` retains the role of the single frame contract. The cost is a small extra delegation step and the need to keep the parameters passed to the assembler aligned with what the backend expects.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](073-advance-scene-systems-through-a-dedicated-frame-updater.md) | [Next](075-resolve-hover-state-through-a-separate-scene-query-service.md)
