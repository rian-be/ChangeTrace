[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](058-render-from-immutable-scene-snapshots.md) | [Next](060-use-deterministic-color-palettes.md)

## [059] Support Manual Camera Control

*2026-05* | Status: accepted

**Context:**

Automatic camera behavior is useful for passive playback, but repository scenes also need inspection, debugging, and selective exploration. The rendering layer therefore needs a camera model that can switch between guided and operator driven movement.

**Problem:**

Automatic tracking alone is not enough to inspect dense or surprising scene states. If camera control lives outside the rendering model, input handling, focus behavior, and scene queries drift apart and become hard to coordinate.

**Decision:**

The renderer exposes manual camera control alongside tracking modes. Camera policy remains part of the rendering layer so that input, scene navigation, and frame assembly share one coherent view of camera state.

**Rejected:**

- Only an automatic camera.
- Managing the camera outside the input/rendering model.
- Driving manual navigation directly from backend window code with no rendering layer policy.
- Treating camera mode as a purely UI concern outside scene behavior.

**Consequences:**

The operator can inspect scenes interactively without abandoning rendering layer invariants. The tradeoff is that camera state and input behavior become more coupled and need explicit coordination with hover, layout, and playback state.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](058-render-from-immutable-scene-snapshots.md) | [Next](060-use-deterministic-color-palettes.md)
