[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](083-render-derived-effects-into-dedicated-gpu-targets.md) | [Next](085-keep-opentk-render-output-as-a-thin-runtime-adapter.md)

## [084] Execute Graphics Runtime Through An Ordered Frame Graph

*2026-06* | Status: accepted

**Context:**

`OpenTkRenderRuntime` does not invoke the scene renderer as one monolithic step. It builds a `RenderFrameGraph`, registers successive `RenderPass` instances in it, and executes them sequentially for each frame.

**Problem:**

The graphics runtime must maintain a stable layer order: background, heatmap, edges, nodes, particles, and HUD. When that order is scattered across many methods or hidden inside one renderer, pass dependencies, profiling, and pipeline rebuilds during resize or resource changes become harder to control.

**Decision:**

A render frame is executed through an explicit sequential frame graph built from `RenderPass`. The runtime composes the graph and passes a shared `RenderFrameContext`, while the logic of individual passes remains isolated.

**Rejected:**

- One renderer executing every stage in one method.
- Encoding pass order directly inside scene-renderer classes.
- Budowanie dynamicznego DAG-a dependencies bez realnej potrzeby w aktualnym runtime.
- Zlewanie layers orkiestracji ramki z implementacja pojedynczych efektow.

**Consequences:**

Pass order and responsibility become explicit, and the runtime can be profiled and rebuilt without touching every renderer. The cost is a larger set of execution objects and the need to preserve the `RenderFrameContext` contract and stable pass ordering.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](083-render-derived-effects-into-dedicated-gpu-targets.md) | [Next](085-keep-opentk-render-output-as-a-thin-runtime-adapter.md)
