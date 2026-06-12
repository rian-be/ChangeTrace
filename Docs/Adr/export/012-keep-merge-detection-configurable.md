[ADR Home](../README.md) | [Category Index](./README.md) | [Next](014-centralize-export-orchestration-in-a-single-repository-exporter.md)

## [012] Keep Merge Detection Configurable

*2026-06* | Status: accepted

**Context:**

Merge detection enriches the timeline with extra structure, but not every repository or export run needs that cost. Some histories are large enough that merge analysis becomes expensive, and some workflows only need the base commit stream.

**Problem:**

If merge detection is always on, export cost and failure surface increase even for runs that do not benefit from it. If it is always off, the exporter cannot produce richer merge aware side data when the operator actually wants it.

**Decision:**

Export exposes configurable merge detection. The caller can enable, disable, or choose merge related behavior explicitly instead of inheriting one global default for every repository.

**Rejected:**

- Always enabling merge detection.
- Removing merge enrichment from export entirely.
- Hiding merge behavior behind implicit repository size heuristics only.
- Deferring all merge policy to downstream consumers after export finishes.

**Consequences:**

The operator can trade export speed against richer merge aware output deliberately. The tradeoff is that CLI, export options, and tests have to preserve one more piece of explicit export policy.

[ADR Home](../README.md) | [Category Index](./README.md) | [Next](014-centralize-export-orchestration-in-a-single-repository-exporter.md)
