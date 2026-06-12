[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](072-use-hive-layout-for-repository-visualization.md) | [Next](074-assemble-and-submit-render-frames-through-a-thin-boundary.md)

## [073] Advance Scene Systems Through A Dedicated Frame Updater

*2026-06* | Status: accepted

**Context:**

`SceneFrameUpdater` owns frame step logic: computing `dt` from player wall time, stepping layout, ticking animation, updating edges, decaying actors, and moving the camera. `RenderingPipeline` delegates updates to it instead of owning every update sequence itself.

**Problem:**

Scene simulation has several separate subsystems, but all depend on the same frame rhythm and player state. Without a dedicated update component, playback semantics, scene simulation, and render state submission quickly mix in one class, making `dt` tuning, time resets, and update order consistency harder.

**Decision:**

All frame level scene updates are executed through a dedicated frame updater. It is responsible for clamping `dt`, adjusting layout step count to playback speed, and maintaining a stable subsystem update order.

**Rejected:**

- Ticking layout, animation, and camera directly in `RenderingPipeline`.
- Using renderer or window time as the source of `dt` instead of wall time reported by the player.
- Advancing layout only once per frame regardless of playback speed.
- Leaving time reset and `paused/idle` logic scattered across multiple classes.

**Consequences:**

Scene update order becomes stable and can be tuned without touching event flushing or frame assembly logic. The cost is another coordination layer whose correctness affects layout, animation, and camera behavior during fast playback and resets.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](072-use-hive-layout-for-repository-visualization.md) | [Next](074-assemble-and-submit-render-frames-through-a-thin-boundary.md)
