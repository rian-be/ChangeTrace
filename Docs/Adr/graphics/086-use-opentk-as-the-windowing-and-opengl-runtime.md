[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](085-keep-opentk-render-output-as-a-thin-runtime-adapter.md) | [Next](087-keep-input-controllers-outside-rendering-state.md)

## [086] Use OpenTK As The Windowing And Opengl Runtime

*2026-05* | Status: accepted

**Context:**

These decisions move from rendering logic to the execution layer: GPU, OpenGL, buffers, windows, assets, and input control. This is the layer where runtime performance and predictability matter, without taking ownership of the domain model.

**Problem:**

The project needs a controlled OpenGL runtime for player and debug windows.
This is no longer a matter of rendering semantics, but of the execution runtime layer: windows, OpenGL, shaders, buffers, atlases, and input handling.

**Decision:**

OpenTK provides the windowing layer, input handling, and graphics context.
Graphics implements rendering decisions at the technical runtime level, but it does not take ownership of domain modeling or timeline semantics.

**Rejected:**

- Wlasny wrapper platformowy.
- Depending on a game engine as the main runtime.
- Treating the GPU and asset pipeline as an implementation detail without explicit data contracts.
- Moving responsibility for input, window runtime behavior, and shader assets into rendering logic or a presentation layer.

**Consequences:**

The graphics runtime stays close to .NET, but it requires platform validation.
This provides high performance and strong control over the GPU pipeline, while increasing the maintenance cost of assets, shaders, and platform-specific behavior.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](085-keep-opentk-render-output-as-a-thin-runtime-adapter.md) | [Next](087-keep-input-controllers-outside-rendering-state.md)
