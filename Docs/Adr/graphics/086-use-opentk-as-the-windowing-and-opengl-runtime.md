[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](085-keep-opentk-render-output-as-a-thin-runtime-adapter.md) | [Next](087-keep-input-controllers-outside-rendering-state.md)

## [086] Use OpenTK As The Windowing And OpenGL Runtime

*2026-05* | Status: accepted

**Context:**

The project needs a runtime that can host debug and player windows, manage OpenGL context lifetime, and integrate cleanly with .NET. That runtime sits below rendering policy and above platform specific window and input behavior.

**Problem:**

The graphics backend needs explicit control over context creation, resize handling, and frame execution, but building a custom platform layer would be expensive and tying the project to a full game engine would import unrelated lifecycle assumptions.

**Decision:**

OpenTK provides the windowing layer, input handling, and OpenGL context for the graphics runtime. Higher level rendering code stays separate, but window and GPU execution rely on OpenTK as the concrete host runtime.

**Rejected:**

- Building a custom platform wrapper for windowing and context management.
- Depending on a game engine as the main runtime.
- Treating the GPU and asset pipeline as an implementation detail without explicit data contracts.
- Moving responsibility for input, window runtime behavior, and shader assets into rendering logic or a presentation layer.

**Consequences:**

The runtime stays close to .NET and gives explicit control over graphics lifecycle behavior. The tradeoff is that platform validation and OpenTK specific runtime concerns become part of the graphics maintenance surface.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](085-keep-opentk-render-output-as-a-thin-runtime-adapter.md) | [Next](087-keep-input-controllers-outside-rendering-state.md)
