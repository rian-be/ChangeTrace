[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](068-share-compute-pipeline-bases-for-gpu-culling-and-texture-generation.md) | [Next](076-build-specialized-buffer-wrappers-on-top-of-a-generic-gpu-buffer.md)

## [069] Use GPU Driven Rendering For Dense Visuals

*2026-05* | Status: accepted

**Context:**

Repository scenes can contain large numbers of edges, particles, labels, and icons. A backend that depends mainly on CPU side per item draw orchestration will hit scaling limits long before the rest of the rendering model does.

**Problem:**

Dense scenes become expensive when the CPU is responsible for preparing and issuing every visual item independently. The graphics runtime needs a model where culling, compaction, and indirect draw preparation can happen closer to the GPU.

**Decision:**

The runtime uses GPU driven rendering for dense visuals. Compute and buffer pipelines prepare draw ready state, and the backend relies on GPU side work to reduce CPU coordination pressure for large scenes.

**Rejected:**

- Drawing every element through a separate high level draw call.
- Keeping scene density handling as a CPU only optimization problem.
- Treating particles, edges, and dense overlays as special case effects outside the main backend model.
- Hiding GPU driven behavior behind one opaque renderer with no explicit data contracts.

**Consequences:**

Dense repository views scale better and the backend can keep more of the heavy draw preparation work on the GPU. The tradeoff is that shaders, buffers, and GPU synchronization become first class architectural concerns rather than incidental backend details.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](068-share-compute-pipeline-bases-for-gpu-culling-and-texture-generation.md) | [Next](076-build-specialized-buffer-wrappers-on-top-of-a-generic-gpu-buffer.md)
