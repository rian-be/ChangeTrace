[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](080-render-labels-and-icons-through-atlas-assets.md) | [Next](082-register-runtime-shaders-through-a-static-manifest.md)

## [081] Keep Shader Sources As Validated Assets

*2026-05* | Status: accepted

**Context:**

Shaders are source assets that define runtime behavior for passes, pipelines, and GPU side effects. They fail differently from regular code changes because compile time coverage is weaker and backend breakage often appears only when a graphics path is exercised.

**Problem:**

Leaving shader files as opaque text artifacts means errors show up late, often only after a window or pass path starts executing. The graphics runtime needs a way to treat shader completeness and validity as part of the asset contract rather than as incidental file presence.

**Decision:**

Shader sources are treated as validated assets. The repository keeps them in the asset workflow, and validation happens through build or CI paths before the runtime is the first place to discover obvious breakage.

**Rejected:**

- Treating shaders as uncontrolled text files.
- Validating only when the window starts.
- Treating shader compile failures as an unavoidable runtime only concern.
- Embedding shader source strings directly in renderer code to avoid asset management.

**Consequences:**

The graphics path becomes more predictable and shader regressions can be caught earlier in development. The tradeoff is that the asset pipeline has to stay maintained as part of normal graphics engineering rather than as an optional convenience.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](080-render-labels-and-icons-through-atlas-assets.md) | [Next](082-register-runtime-shaders-through-a-static-manifest.md)
