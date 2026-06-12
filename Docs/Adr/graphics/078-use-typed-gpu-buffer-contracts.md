[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](077-keep-buffer-lifecycle-and-binding-logic-out-of-renderers-and-pipelines.md) | [Next](079-provide-runtime-fallbacks-for-text-and-icon-assets.md)

## [078] Use Typed GPU Buffer Contracts

*2026-05* | Status: accepted

**Context:**

These decisions move from rendering logic to the execution layer: GPU, OpenGL, buffers, windows, assets, and input control. This is the layer where runtime performance and predictability matter, without taking ownership of the domain model.

**Problem:**

CPU and GPU must agree on data layout.
This is no longer a matter of rendering semantics, but of the execution runtime layer: windows, OpenGL, shaders, buffers, atlases, and input handling.

**Decision:**

GPU data is described through explicit contracts and buffer wrappers.
Graphics implements rendering decisions at the technical runtime level, but it does not take ownership of domain modeling or timeline semantics.

**Rejected:**

- Surowe tablice float bez modelu.
- Encoding data layout only in the shader.
- Treating the GPU and asset pipeline as an implementation detail without explicit data contracts.
- Moving responsibility for input, window runtime behavior, and shader assets into rendering logic or a presentation layer.

**Consequences:**

Mniej errors layoutu, but changes struktur require synchronizacji z shaderami.
This provides high performance and strong control over the GPU pipeline, while increasing the maintenance cost of assets, shaders, and platform-specific behavior.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](077-keep-buffer-lifecycle-and-binding-logic-out-of-renderers-and-pipelines.md) | [Next](079-provide-runtime-fallbacks-for-text-and-icon-assets.md)
