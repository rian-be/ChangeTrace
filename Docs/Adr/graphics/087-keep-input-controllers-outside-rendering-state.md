[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](086-use-opentk-as-the-windowing-and-opengl-runtime.md) | [Next](088-use-debug-windows-for-rendering-development.md)

## [087] Keep Input Controllers Outside Rendering State

*2026-05* | Status: accepted

**Context:**

These decisions move from rendering logic to the execution layer: GPU, OpenGL, buffers, windows, assets, and input control. This is the layer where runtime performance and predictability matter, without taking ownership of the domain model.

**Problem:**

Keyboard and mouse input express operator intent rather than forming part of scene state.
This is no longer a matter of rendering semantics, but of the execution runtime layer: windows, OpenGL, shaders, buffers, atlases, and input handling.

**Decision:**

Input is handled by dedicated controllers.
Graphics implements rendering decisions at the technical runtime level, but it does not take ownership of domain modeling or timeline semantics.

**Rejected:**

- Handling input inside the scene graph.
- Spreading keyboard conditions across render outputs.
- Treating the GPU and asset pipeline as an implementation detail without explicit data contracts.
- Moving responsibility for input, window runtime behavior, and shader assets into rendering logic or a presentation layer.

**Consequences:**

Input control becomes easier to maintain, but input mapping must stay consistent with the player and camera.
This provides high performance and strong control over the GPU pipeline, while increasing the maintenance cost of assets, shaders, and platform-specific behavior.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](086-use-opentk-as-the-windowing-and-opengl-runtime.md) | [Next](088-use-debug-windows-for-rendering-development.md)
