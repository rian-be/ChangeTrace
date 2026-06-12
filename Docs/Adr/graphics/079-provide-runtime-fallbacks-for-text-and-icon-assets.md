[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](078-use-typed-gpu-buffer-contracts.md) | [Next](080-render-labels-and-icons-through-atlas-assets.md)

## [079] Provide Runtime Fallbacks For Text And Icon Assets

*2026-06* | Status: accepted

**Context:**

The text and icon runtime does not assume that every asset is always present on disk. `FontAtlasLoader` can generate a built-in glyph atlas, and `LanguageIconAtlasLoader` searches several locations and can degrade cleanly to a no-icons state.

**Problem:**

The graphics backend should not be inseparably tied to the presence of one asset set in one specific directory. Missing a text atlas or icons must not automatically disable the entire runtime, especially in local, development, and debug scenarios.

**Decision:**

Graphics maintains runtime fallbacks for critical text assets and controlled degradation for optional assets. Text has a built-in recovery path, while language icons are treated as an optional resource.

**Rejected:**

- Failing initialization hard when any bitmap asset is missing.
- Assuming one fixed location for every atlas.
- Generating fallbacks only in a higher rendering layer.
- Mixing asset-resolution logic into scene and HUD renderers.

**Consequences:**

The runtime becomes easier to start in different environments and modes, and one missing asset does not stop the whole visualization. The tradeoff is maintaining two behavior paths: a full path with assets and a degraded path with fallbacks or no icons.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](078-use-typed-gpu-buffer-contracts.md) | [Next](080-render-labels-and-icons-through-atlas-assets.md)
