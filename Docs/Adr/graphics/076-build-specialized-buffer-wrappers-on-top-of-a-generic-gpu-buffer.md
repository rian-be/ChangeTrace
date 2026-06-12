[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](069-use-gpu-driven-rendering-for-dense-visuals.md) | [Next](077-keep-buffer-lifecycle-and-binding-logic-out-of-renderers-and-pipelines.md)

## [076] Build Specialized Buffer Wrappers On Top Of A Generic GPU Buffer

*2026-06* | Status: accepted

**Context:**

The `ChangeTrace.Graphics.Gpu.Buffers` layer has a shared core in the form of `GpuBuffer<T>`, but it does not expose it as the only public runtime abstraction. On top of it sit specialized wrappers such as `ShaderStorageBuffer<T>`, `VertexBuffer<T>`, `IndexBuffer<TCommand>`, and `IndirectDrawBuffer<TCommand>`, each with an assigned OpenGL target and operations specific to its use case.

**Problem:**

GPU buffers share allocation, upload, and disposal mechanics, but they differ in target semantics, binding patterns, and where they are used in the pipeline. Using only one generic `GpuBuffer<T>` wrapper throughout the runtime would force renderers and pipelines to know on every call which target they are working with and which operations are valid for it.

**Decision:**

The low-level `GpuBuffer<T>` remains the shared core for lifecycle and upload, while the runtime mostly uses specialized wrappers for different OpenGL buffer classes. Buffer-type semantics are encoded in the `Gpu.Buffers` layer instead of being scattered across renderers and compute pipelines.

**Rejected:**

- Using one universal `GpuBuffer<T>` directly from every pipeline and renderer.
- Separate, duplicated allocation and upload implementations for every buffer type.
- Encoding the OpenGL target as a loose parameter passed everywhere from the outside.
- Combining GPU data contracts with responsibility for raw buffer operations in the same classes.

**Consequences:**

The buffer layer becomes more readable and enforces more correct usage semantics on the renderer and pipeline side. The cost is a larger number of small wrappers that must stay aligned with the `GpuBuffer<T>` core and the real OpenGL usage model.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](069-use-gpu-driven-rendering-for-dense-visuals.md) | [Next](077-keep-buffer-lifecycle-and-binding-logic-out-of-renderers-and-pipelines.md)
