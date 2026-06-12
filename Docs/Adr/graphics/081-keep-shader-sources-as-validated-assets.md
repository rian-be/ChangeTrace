[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](080-render-labels-and-icons-through-atlas-assets.md) | [Next](082-register-runtime-shaders-through-a-static-manifest.md)

## [081] Keep Shader Sources As Validated Assets

*2026-05* | Status: accepted

**Context:**

These decisions move from rendering logic to the execution layer: GPU, OpenGL, buffers, windows, assets, and input control. This is the layer where runtime performance and predictability matter, without taking ownership of the domain model.

**Problem:**

Shader errors should be detected before runtime when possible.
This is no longer a matter of rendering semantics, but of the execution runtime layer: windows, OpenGL, shaders, buffers, atlases, and input handling.

**Decision:**

Shader sources are treated as assets and validated by the workflow.
Graphics implements rendering decisions at the technical runtime level, but it does not take ownership of domain modeling or timeline semantics.

**Rejected:**

- Treating shaders as uncontrolled text files.
- Validating only when the window starts.
- Treating the GPU and asset pipeline as an implementation detail without explicit data contracts.
- Moving responsibility for input, window runtime behavior, and shader assets into rendering logic or a presentation layer.

**Consequences:**

CI can catch some graphics errors, but the asset pipeline still has to be maintained.
This provides high performance and strong control over the GPU pipeline, while increasing the maintenance cost of assets, shaders, and platform-specific behavior.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](080-render-labels-and-icons-through-atlas-assets.md) | [Next](082-register-runtime-shaders-through-a-static-manifest.md)
