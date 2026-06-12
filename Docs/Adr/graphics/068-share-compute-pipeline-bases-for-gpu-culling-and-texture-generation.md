[ADR Home](../README.md) | [Category Index](./README.md) | [Next](069-use-gpu-driven-rendering-for-dense-visuals.md)

## [068] Share Compute Pipeline Bases For GPU Culling And Texture Generation

*2026-06* | Status: accepted

**Context:**

The `graphics/` code maintains two shared compute-pipeline patterns: `IndirectComputePipeline<TGpuItem>` for compaction and indirect draw, and `TextureComputePipeline<TData>` for texture generation or processing. Concrete pipelines inherit from those bases instead of reimplementing synchronization and binding from scratch.

**Problem:**

GPU pipelines for circles, edges, text, particles, and heatmaps share the same set of low-level operations: upload, storage-buffer binding, dispatch, memory barriers, and indirect-draw or image-texture state reset. Duplicating those sequences in every pipeline increases the risk of inconsistent barriers and hard-to-detect GPU synchronization errors.

**Decision:**

Shared compute mechanics are kept in base pipeline classes, while concrete pipelines add only data contracts and specific shaders. This applies both to pipelines that compact data for indirect draw and to pipelines that produce derived textures.

**Rejected:**

- Reimplementing binding, dispatch, and memory barriers in every pipeline separately.
- Jeden uniwerarelny pipeline dla wszystkich typow workloadu GPU.
- Ukrycie contractow compute za ogolnym rendererem wysokiego levelu.
- Moving GPU synchronization responsibilities into pass layers or the window runtime.

**Consequences:**

GPU synchronization and the compute-work pattern remain consistent across pipelines, and new workloads are easier to attach to the existing architecture. In return, the base classes become an important technical contract and must not drift into an overly generic abstraction detached from real runtime needs.

[ADR Home](../README.md) | [Category Index](./README.md) | [Next](069-use-gpu-driven-rendering-for-dense-visuals.md)
