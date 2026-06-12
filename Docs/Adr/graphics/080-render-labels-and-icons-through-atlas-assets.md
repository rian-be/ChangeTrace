[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](079-provide-runtime-fallbacks-for-text-and-icon-assets.md) | [Next](081-keep-shader-sources-as-validated-assets.md)

## [080] Render Labels And Icons Through Atlas Assets

*2026-05* | Status: accepted

**Context:**

Labels and language icons are rendered frequently across HUD and scene overlays. Issuing separate texture resources or draw setup for each glyph and icon would add unnecessary overhead to a runtime that already handles dense visual layers.

**Problem:**

Text and icon rendering become expensive if every asset is resolved and bound independently at runtime. The runtime needs one predictable resource layout that supports repeated label drawing without turning UI overlays into a stream of tiny texture operations.

**Decision:**

The renderer uses glyph and language icon atlases. Text and icon lookups resolve into atlas coordinates, and the backend draws those assets through shared texture resources rather than per item bitmap loading.

**Rejected:**

- Loading icons one by one at runtime.
- Having no language icons in the visualization.
- Generating per label textures during normal frame execution.
- Treating text and icon assets as unrelated runtime paths with separate draw models.

**Consequences:**

Label and icon rendering become cheaper and more predictable under repeated use. The tradeoff is that atlas generation, versioning, and fallback behavior become explicit asset management concerns inside the graphics runtime.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](079-provide-runtime-fallbacks-for-text-and-icon-assets.md) | [Next](081-keep-shader-sources-as-validated-assets.md)
