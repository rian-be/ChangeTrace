[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](079-provide-runtime-fallbacks-for-text-and-icon-assets.md) | [Next](081-keep-shader-sources-as-validated-assets.md)

## [080] Render Labels And Icons Through Atlas Assets

*2026-05* | Status: accepted

**Context:**

These decisions move from rendering logic to the execution layer: GPU, OpenGL, buffers, windows, assets, and input control. This is the layer where runtime performance and predictability matter, without taking ownership of the domain model.

**Problem:**

Text and icons are common in the HUD and scene, and individual assets would be expensive.
This is no longer a matter of rendering semantics, but of the execution runtime layer: windows, OpenGL, shaders, buffers, atlases, and input handling.

**Decision:**

The renderer uses glyph and language-icon atlases.
Graphics implements rendering decisions at the technical runtime level, but it does not take ownership of domain modeling or timeline semantics.

**Rejected:**

- Loading icons one by one at runtime.
- Having no language icons in the visualization.
- Treating the GPU and asset pipeline as an implementation detail without explicit data contracts.
- Moving responsibility for input, window runtime behavior, and shader assets into rendering logic or a presentation layer.

**Consequences:**

Label rendering becomes more efficient, but the atlas must be generated and versioned.
This provides high performance and strong control over the GPU pipeline, while increasing the maintenance cost of assets, shaders, and platform-specific behavior.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](079-provide-runtime-fallbacks-for-text-and-icon-assets.md) | [Next](081-keep-shader-sources-as-validated-assets.md)
