[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](082-register-runtime-shaders-through-a-static-manifest.md) | [Next](084-execute-graphics-runtime-through-an-ordered-frame-graph.md)

## [083] Render Derived Effects Into Dedicated GPU Targets

*2026-06* | Status: accepted

**Context:**

The graphics runtime maintains separate GPU targets for derived effects instead of treating them as direct drawing into the main backbuffer. The code uses explicit textures and framebuffers for stages such as trail accumulation, heatmaps, and bloom masks.

**Problem:**

Accumulation and post-process effects need their own resource lifecycle, resolution, and synchronization model. When they are computed directly in the main draw path, resize handling, texture reuse, and composition of intermediate-data stages become harder to manage.

**Decision:**

Derived effects are rendered or generated into dedicated GPU targets and only then consumed by subsequent passes. This applies both to framebuffers drawn by fullscreen passes and to textures written by compute shaders.

**Rejected:**

- Computing every effect directly in the main draw path without intermediate resources.
- Keeping effect data on the CPU side between passes.
- Mieszanie lifecycle targetow efektow z podstawowymi buforami geometrii.
- Creating temporary targets ad hoc inside renderers instead of maintaining explicit resource components.

**Consequences:**

The effects pipeline becomes more modular and handles resize and implementation swaps better. The tradeoff is a larger number of GPU resources and stricter discipline around texture formats, framebuffers, and memory barriers.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](082-register-runtime-shaders-through-a-static-manifest.md) | [Next](084-execute-graphics-runtime-through-an-ordered-frame-graph.md)
