[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](068-share-compute-pipeline-bases-for-gpu-culling-and-texture-generation.md) | [Next](076-build-specialized-buffer-wrappers-on-top-of-a-generic-gpu-buffer.md)

## [069] Use GPU Driven Rendering For Dense Visuals

*2026-05* | Status: accepted

**Context:**

These decisions move from rendering logic to the execution layer: GPU, OpenGL, buffers, windows, assets, and input control. This is the layer where runtime performance and predictability matter, without taking ownership of the domain model.

**Problem:**

Repositories can generate many edges, particles, text labels, and icons.
This is no longer a matter of rendering semantics, but of the execution runtime layer: windows, OpenGL, shaders, buffers, atlases, and input handling.

**Decision:**

Docelowa path graficzna korzysta z GPU-driven pipelines.
Graphics implements rendering decisions at the technical runtime level, but it does not take ownership of domain modeling or timeline semantics.

**Rejected:**

- CPU rendering jako main path.
- Drawing every element through a separate high-level draw call.
- Treating the GPU and asset pipeline as an implementation detail without explicit data contracts.
- Moving responsibility for input, window runtime behavior, and shader assets into rendering logic or a presentation layer.

**Consequences:**

Visualization performance improves, but shaders and buffers become part of the architecture.
This provides high performance and strong control over the GPU pipeline, while increasing the maintenance cost of assets, shaders, and platform-specific behavior.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](068-share-compute-pipeline-bases-for-gpu-culling-and-texture-generation.md) | [Next](076-build-specialized-buffer-wrappers-on-top-of-a-generic-gpu-buffer.md)
