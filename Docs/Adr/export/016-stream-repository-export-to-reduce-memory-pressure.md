[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](015-record-export-stage-completion-explicitly.md) | [Next](091-keep-libgit2sharp-as-a-backend-option.md)

## [016] Stream Repository Export To Reduce Memory Pressure

*2026-06* | Status: accepted

**Context:**

Repository export can process histories that are large enough to make full in memory timeline construction expensive. The exporter already stages work across reading, building, and persistence, so it has a natural place to adopt a more streaming oriented path.

**Problem:**

Large repositories become costly if the whole timeline is materialized before persistence begins. That increases managed memory pressure and delays failure visibility until late in the export.

**Decision:**

The exporter can stream repository events directly into `.gittrace`-oriented persistence paths instead of requiring one fully materialized in memory timeline first. Streaming is treated as an export mode decision inside the main pipeline rather than as a separate one off code path.

**Rejected:**

- Fully materializing every export before writing.
- Optimizing only the serializers while leaving the exporter fully batch oriented.
- Treating streaming as a CLI only shortcut outside the main exporter.
- Splitting export into unrelated “streaming” and “regular” orchestrators.

**Consequences:**

Memory usage drops and long running exports can fail or checkpoint earlier in the pipeline. The tradeoff is that progressive persistence and in flight failure handling become part of exporter correctness, not just a persistence concern.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](015-record-export-stage-completion-explicitly.md) | [Next](091-keep-libgit2sharp-as-a-backend-option.md)
