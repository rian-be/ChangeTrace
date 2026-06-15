[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](099-write-sidecars-through-dedicated-handlers.md)

## [100] Use Checkpoints For Resumable Export

*2026-06* | Status: accepted

**Context:**

Large exports can take long enough that interruption is a normal operating condition, not an edge case. Once export spans multiple stages and multiple artifacts, restart from zero becomes both slow and operationally wasteful.

**Problem:**

Long exports should not restart from zero after interruption. Without checkpoints, the exporter has to repeat repository reading, enrichment, and persistence work even when earlier stages completed successfully.

**Decision:**

Export maintains stage checkpoints and can resume work from those saved points. Checkpoints are part of exporter state rather than a side effect inferred from whatever files happen to exist.

**Rejected:**

- All or nothing export.
- Treating partial files as an implicit checkpoint.
- Rebuilding exporter state only from logs and artifact timestamps.
- Moving resume support into an external wrapper instead of the exporter itself.

**Consequences:**

Resume becomes a first class export behavior and interruption cost drops for long running runs. The tradeoff is that checkpoint state becomes another durable contract that must stay compatible with exporter evolution and artifact layout changes.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](099-write-sidecars-through-dedicated-handlers.md)
