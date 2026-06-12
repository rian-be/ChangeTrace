[ADR Home](../README.md)

# rendering

This category covers the logical rendering pipeline from events to scene state and layout.

## Scope

Look here for render commands, scene graph, snapshots, render state, translation, frame assembly, and layout as a visual model independent of the GPU runtime.

## Responsibility Boundaries

Rendering should not define domain meaning and should not absorb OpenGL runtime concerns. It is the middle layer between domain and graphics.

## How To Start Reading

Start here when changing scene behavior, translators, layout, or render-state assembly.

## ADR List

| ADR | Title |
| --- | --- |
| [050-separate-rendering-core-from-graphics-runtime.md](./050-separate-rendering-core-from-graphics-runtime.md) | Separate Rendering Core From Graphics Runtime |
| [051-translate-trace-events-into-render-commands.md](./051-translate-trace-events-into-render-commands.md) | Translate Trace Events Into Render Commands |
| [052-use-render-commands-as-visual-intent.md](./052-use-render-commands-as-visual-intent.md) | Use Render Commands As Visual Intent |
| [055-order-event-translators-by-explicit-priority.md](./055-order-event-translators-by-explicit-priority.md) | Order Event Translators By Explicit Priority |
| [056-keep-animation-as-a-subsystem.md](./056-keep-animation-as-a-subsystem.md) | Keep Animation As A Subsystem |
| [057-maintain-a-scene-graph-for-visualization-state.md](./057-maintain-a-scene-graph-for-visualization-state.md) | Maintain A Scene Graph For Visualization State |
| [058-render-from-immutable-scene-snapshots.md](./058-render-from-immutable-scene-snapshots.md) | Render From Immutable Scene Snapshots |
| [059-support-manual-camera-control.md](./059-support-manual-camera-control.md) | Support Manual Camera Control |
| [060-use-deterministic-color-palettes.md](./060-use-deterministic-color-palettes.md) | Use Deterministic Color Pbutttes |
| [061-use-render-state-assembly-as-a-boundary.md](./061-use-render-state-assembly-as-a-boundary.md) | Use Render State Assembly As A Boundary |
| [066-buffer-and-aggregate-playback-events-before-scene-dispatch.md](./066-buffer-and-aggregate-playback-events-before-scene-dispatch.md) | Buffer And Aggregate Playback Events Before Scene Dispatch |
| [067-route-renderable-semantic-events-through-an-explicit-dispatch-table.md](./067-route-renderable-semantic-events-through-an-explicit-dispatch-table.md) | Route Renderable Semantic Events Through An Explicit Dispatch Table |
| [070-represent-scene-relations-explicitly.md](./070-represent-scene-relations-explicitly.md) | Represent Scene Relations Explicitly |
| [071-use-flyweight-backed-scene-nodes.md](./071-use-flyweight-backed-scene-nodes.md) | Use Flyweight Backed Scene Nodes |
| [072-use-hive-layout-for-repository-visualization.md](./072-use-hive-layout-for-repository-visualization.md) | Use Hive Layout For Repository Visualization |
| [073-advance-scene-systems-through-a-dedicated-frame-updater.md](./073-advance-scene-systems-through-a-dedicated-frame-updater.md) | Advance Scene Systems Through A Dedicated Frame Updater |
| [074-assemble-and-submit-render-frames-through-a-thin-boundary.md](./074-assemble-and-submit-render-frames-through-a-thin-boundary.md) | Assemble And Submit Render Frames Through A Thin Boundary |
| [075-resolve-hover-state-through-a-separate-scene-query-service.md](./075-resolve-hover-state-through-a-separate-scene-query-service.md) | Resolve Hover State Through A Separate Scene Query Service |
