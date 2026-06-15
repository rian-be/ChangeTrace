[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](084-execute-graphics-runtime-through-an-ordered-frame-graph.md) | [Next](086-use-opentk-as-the-windowing-and-opengl-runtime.md)

## [085] Keep OpenTK Render Output As A Thin Runtime Adapter

*2026-06* | Status: accepted

**Context:**

`OpenTkRenderOutput` implements the `IRenderOutput` contract, but it does not take ownership of pass composition, pipelines, or GPU resources. That logic stays in `OpenTkRenderRuntime`, while the output acts as a lifecycle and `RenderState` adapter.

**Problem:**

The integration with `Rendering` needs a stable output contract, but it should not receive the full details of OpenGL, passes, and shaders. When adapter and runtime are merged into one object, the boundary between the system contract and graphics backend details erodes quickly.

**Decision:**

`IRenderOutput` remains a thin adapter over the OpenTK backend, while all execution details stay behind a separate runtime class. The adapter is responsible only for `Initialize`, `Submit`, `Resize`, and `Dispose`.

**Rejected:**

- Exposing `OpenTkRenderRuntime` directly as the implementation of a higher layer contract.
- Moving pass and pipeline construction logic into the output adapter.
- Coupling the `IRenderOutput` contract to APIs specific to OpenGL or OpenTK.
- Spreading renderer lifecycle across the window, pipeline, and runtime without one boundary class.

**Consequences:**

The `rendering/` layer sees a simple output contract, while the OpenTK backend can evolve without changing the higher layer interface. The drawback is an extra delegation layer that must remain aligned with the runtime and its lifecycle.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](084-execute-graphics-runtime-through-an-ordered-frame-graph.md) | [Next](086-use-opentk-as-the-windowing-and-opengl-runtime.md)
