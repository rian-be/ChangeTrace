[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](074-assemble-and-submit-render-frames-through-a-thin-boundary.md)

## [075] Resolve Hover State Through A Separate Scene Query Service

*2026-06* | Status: accepted

**Context:**

`HoverPickingService` keeps mouse position state, converts screen coordinates into world space, finds the nearest `SceneNode`, and optionally resolves an active hive pod. The rendering pipeline uses it as a separate data source for the HUD instead of mixing hit testing with scene updates or the output backend.

**Problem:**

Hover and picking are logically tied to the scene, camera, and layout, but they are not part of scene simulation itself or the graphics backend. When hit testing is hidden in the renderer, window, or node snapshots, responsibility for interaction, scene representation, and HUD presentation gets mixed quickly.

**Decision:**

Hover state resolution is maintained as a separate query service over scene and layout. The pipeline passes cursor changes into it, and the result feeds HUD assembly and renderer state without mutating the scene graph.

**Rejected:**

- Performing hit testing directly in the `graphics/` backend.
- Keeping hover state inside `SceneNode` or `CameraController`.
- Computing hover only during HUD rendering instead of before frame assembly.
- Separate, inconsistent picking implementations for nodes and hive pods.

**Consequences:**

Interaction remains part of the rendering layer, but it does not contaminate the scene model or the output backend. The cost is an extra component that depends on the scene, camera, and layout and must remain aligned with zoom, rotation, and layout mode semantics.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](074-assemble-and-submit-render-frames-through-a-thin-boundary.md)
