[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](020-keep-provider-specific-enrichment-behind-interfaces.md) | [Next](022-skip-unavailable-github-pr-data-without-failing-export.md)

## [021] Make Enrichment Optional Per Export

*2026-06* | Status: accepted

**Context:**

Repository export can succeed without host enrichment, but enrichment may still be useful for pull request timelines, merge analysis, and richer sidecars. Different export runs also have different tolerance for API latency, auth requirements, and external failures.

**Problem:**

Treating enrichment as mandatory forces every export through the slowest and least reliable part of the pipeline, even when the operator only needs the base timeline. It also removes control over failure modes when provider APIs are unavailable or unnecessary for the current run.

**Decision:**

Enrichment is selectable per export through explicit options and prompts. The exporter receives a clear enrichment scope and can skip provider work entirely when the selected mode does not require it.

**Rejected:**

- Always attempting full enrichment.
- Hiding enrichment from the calling layer.
- Binding enrichment policy permanently to one repository or workspace setting.
- Deriving enrichment behavior implicitly from whichever provider credentials happen to be available.

**Consequences:**

Export cost, speed, and risk become controllable at runtime, and automated invocations can choose a narrower scope deliberately. The tradeoff is that CLI and exporter contracts must carry explicit enrichment intent instead of assuming one universal export mode.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](020-keep-provider-specific-enrichment-behind-interfaces.md) | [Next](022-skip-unavailable-github-pr-data-without-failing-export.md)
