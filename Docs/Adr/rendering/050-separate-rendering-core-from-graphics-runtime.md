[ADR Home](../README.md) | [Category Index](./README.md) | [Next](051-translate-trace-events-into-render-commands.md)

## [050] Separate Rendering Core From Graphics Runtime

*2026-05* | Status: accepted

**Context:**

The repository needs one layer that decides what should be visualized and another that decides how GPU and windowing code executes that plan. Scene state, layout, camera, and render commands change for different visualization policies even when the OpenGL backend stays the same.

**Problem:**

If rendering policy and graphics execution live in one layer, every scene level change drags in GPU runtime concerns and every backend change risks altering visualization semantics. That makes testing harder and ties scene evolution to a specific runtime implementation.

**Decision:**

Rendering core is kept separate from the graphics runtime. The rendering layer owns scene logic, translation, snapshots, and render state, while the graphics layer consumes that prepared state through a backend contract.

**Rejected:**

- One layer combining scene semantics and GPU execution.
- Letting OpenTK types leak directly into scene and translation code.
- Building visualization behavior straight from timeline events inside backend renderers.
- Treating graphics runtime details as the natural home for layout and camera policy.

**Consequences:**

Scene logic can be tested and evolved independently from GPU execution, and backend changes stay more localized. The tradeoff is a larger boundary surface between rendering and graphics that has to remain coherent over time.

[ADR Home](../README.md) | [Category Index](./README.md) | [Next](051-translate-trace-events-into-render-commands.md)
