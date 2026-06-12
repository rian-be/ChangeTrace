[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](009-build-timelines-through-a-dedicated-builder.md) | [Next](011-optimize-timeline-builder-hot-paths.md)

## [010] Model Repository Activity As Trace Events

*2026-02* | Status: accepted

**Context:**

This decision concerns the language the system uses to describe the repository and its history. It is the layer used by export, the player, aggregation, and rendering, so its goal is not the convenience of one module but a shared and stable model for all of them.

**Problem:**

Different parts of the application need a shared language for describing repository activity.
In practice, the goal is to prevent raw Git history from leaking into other layers as an unstructured set of strings, dates, and ad hoc structures.

**Decision:**

Aktywnosc repository is reprezentowana jako TraceEvent z metadanymi domainmi.
The responsibility boundary here runs between the domain model and infrastructure: export, providers, the player, and rendering consume the established model instead of defining it independently.

**Rejected:**

- Separate event models for CLI, export, and rendering.
- Bezposrednie uarege rekordow historii Git.
- Reinterpreting the domain model locally in each layer.
- A hybrid in which part of the meaning remains in Core and part in infrastructure or the presentation layer.

**Consequences:**

`TraceEvent` becomes a central system contract and requires careful evolution when format or aggregation changes.
This decision shapes contracts across many modules, so every change in Core requires attention to compatibility and the refactoring cost outside the core itself.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](009-build-timelines-through-a-dedicated-builder.md) | [Next](011-optimize-timeline-builder-hot-paths.md)
