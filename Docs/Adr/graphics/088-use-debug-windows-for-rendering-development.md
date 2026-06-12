[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](087-keep-input-controllers-outside-rendering-state.md)

## [088] Use Debug Windows For Rendering Development

*2026-05* | Status: accepted

**Context:**

These decisions move from rendering logic to the execution layer: GPU, OpenGL, buffers, windows, assets, and input control. This is the layer where runtime performance and predictability matter, without taking ownership of the domain model.

**Problem:**

A CLI command alone is not enough to diagnose interactive graphics behavior.
This is no longer a matter of rendering semantics, but of the execution runtime layer: windows, OpenGL, shaders, buffers, atlases, and input handling.

**Decision:**

The project exposes debug and player windows for runtime inspection.
Graphics implements rendering decisions at the technical runtime level, but it does not take ownership of domain modeling or timeline semantics.

**Rejected:**

- Debugging only through logs.
- Using the production window as the only way to test graphics.
- Treating the GPU and asset pipeline as an implementation detail without explicit data contracts.
- Moving responsibility for input, window runtime behavior, and shader assets into rendering logic or a presentation layer.

**Consequences:**

Rendering development becomes faster, but debug windows must stay isolated from the core workflow.
This provides high performance and strong control over the GPU pipeline, while increasing the maintenance cost of assets, shaders, and platform-specific behavior.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](087-keep-input-controllers-outside-rendering-state.md)
