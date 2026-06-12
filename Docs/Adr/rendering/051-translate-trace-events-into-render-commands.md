[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](050-separate-rendering-core-from-graphics-runtime.md) | [Next](052-use-render-commands-as-visual-intent.md)

## [051] Translate Trace Events Into Render Commands

*2026-02* | Status: accepted

**Context:**

The timeline describes repository activity, but the renderer needs operations that can update scene state consistently across different event kinds. Translation sits between those two concerns and turns event meaning into something the scene can apply deterministically.

**Problem:**

If the renderer reads raw timeline events directly, visual policy ends up embedded in event handling branches spread across the pipeline. That makes it hard to introduce new visual behaviors without coupling them to timeline storage details.

**Decision:**

Trace events are translated into render commands through dedicated translators. Translators isolate event interpretation from scene mutation and let the renderer consume a visual operation layer instead of raw timeline history.

**Rejected:**

- Letting the renderer mutate scene state directly from `TraceEvent`.
- Embedding visual fields into the base timeline model.
- Building every visual rule as a special case branch inside the pipeline.
- Treating translation as a thin naming layer without its own semantics.

**Consequences:**

Event interpretation becomes easier to extend and test separately from scene execution. The tradeoff is that translators become a central policy layer whose behavior must remain aligned with both the timeline model and the scene contracts.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](050-separate-rendering-core-from-graphics-runtime.md) | [Next](052-use-render-commands-as-visual-intent.md)
