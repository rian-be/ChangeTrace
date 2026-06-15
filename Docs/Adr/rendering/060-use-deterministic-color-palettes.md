[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](059-support-manual-camera-control.md) | [Next](061-use-render-state-assembly-as-a-boundary.md)

## [060] Use Deterministic Color Palettes

*2026-05* | Status: accepted

**Context:**

Color is part of how the visualization communicates actor identity, file identity, and scene continuity. If color assignment shifts unpredictably between runs, the renderer loses one of its main tools for building stable visual recognition.

**Problem:**

Colors for actors, files, and edges should remain repeatable across runs. Random or frame local color assignment makes screenshots, comparisons, and operator recognition less useful and turns color into decoration rather than a consistent signal.

**Decision:**

Color palettes are deterministic and derived from domain data. Rendering uses stable mapping rules so that the same entities resolve to the same palette behavior unless the underlying domain identity changes.

**Rejected:**

- Randomizing colors on each run.
- Assigning colors from transient scene order or render time allocation order.
- Treating color as a purely theme level concern with no domain relationship.
- Storing final colors as mutable backend only state with no deterministic mapping rule.

**Consequences:**

Visualization becomes more stable for repeated inspection, screenshots, and debugging. The tradeoff is that palette design must scale to large repositories without collapsing into visually indistinguishable assignments.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](059-support-manual-camera-control.md) | [Next](061-use-render-state-assembly-as-a-boundary.md)
