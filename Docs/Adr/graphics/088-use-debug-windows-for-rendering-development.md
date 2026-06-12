[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](087-keep-input-controllers-outside-rendering-state.md)

## [088] Use Debug Windows For Rendering Development

*2026-05* | Status: accepted

**Context:**

Rendering behavior is hard to diagnose through logs alone because many failures are spatial, temporal, or resource dependent. The graphics runtime benefits from dedicated windows that can expose scene state, debug overlays, and backend behavior interactively.

**Problem:**

A CLI command alone is not enough to inspect frame composition, hover behavior, layout drift, or pass level output. Without dedicated debug windows, graphics diagnosis gets pushed into ad hoc logging and production player workflows that are too indirect for rapid iteration.

**Decision:**

The project exposes debug and player windows for runtime inspection. Rendering development and diagnostics can exercise graphics behavior through dedicated runtime surfaces instead of relying only on the normal production facing path.

**Rejected:**

- Debugging only through logs.
- Using the production window as the only way to test graphics.
- Treating debug visualization as an external only tool outside the runtime.
- Folding every inspection need into the main player surface with no separate diagnostics path.

**Consequences:**

Rendering development becomes faster and graphics issues become easier to localize interactively. The tradeoff is that debug windows add more runtime surfaces whose lifecycle and diagnostic behavior must stay isolated from the main workflow.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](087-keep-input-controllers-outside-rendering-state.md)
