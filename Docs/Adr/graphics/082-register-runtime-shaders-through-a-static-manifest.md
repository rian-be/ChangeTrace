[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](081-keep-shader-sources-as-validated-assets.md) | [Next](083-render-derived-effects-into-dedicated-gpu-targets.md)

## [082] Register Runtime Shaders Through A Static Manifest

*2026-06* | Status: accepted

**Context:**

The shader layer maintains an explicit `ShaderManifest` that registers runtime shader names, their `ShaderKind`, and the corresponding asset paths. The runtime does not scan shader directories dynamically at startup and does not rely on ad hoc file references from individual renderers.

**Problem:**

The graphics backend needs a stable `name -> shader definition` map so that passes, pipelines, and runtime resources can request concrete programs without knowing the physical file locations. Without a central manifest, shader knowledge gets scattered across renderers, and renaming a shader or changing its kind becomes difficult to trace.

**Decision:**

Runtime shaders are registered through a static manifest of definitions, which forms a contract between assets, the shader loader, and the graphics layer. A shader name becomes a stable runtime identifier rather than just a file name known to a single renderer.

**Rejected:**

- Dynamically scanning shader directories on every startup.
- Direct references to raw shader asset paths from renderers.
- Separate local shader registries maintained by each pipeline.
- Coupling shader kind and path knowledge directly to concrete pass code.

**Consequences:**

The shader runtime becomes more predictable and it is easier to control asset completeness and naming consistency across the graphics backend. The cost is having to maintain the manifest as a central contract and update it whenever a new runtime shader is added.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](081-keep-shader-sources-as-validated-assets.md) | [Next](083-render-derived-effects-into-dedicated-gpu-targets.md)
