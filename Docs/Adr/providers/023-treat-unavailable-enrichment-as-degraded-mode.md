[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](022-skip-unavailable-github-pr-data-without-failing-export.md) | [Next](033-support-provider-aware-login-flows.md)

## [023] Treat Unavailable Enrichment As Degraded Mode

*2026-06* | Status: accepted

**Context:**

Host enrichment extends the export model, but it is not the same thing as the core timeline derived from Git history. The exporter needs a way to represent partial success when optional provider data is missing while the main artifact remains valid.

**Problem:**

Without an explicit degraded mode model, the exporter has only two outcomes: full success or hard failure. That forces optional host data into the same success semantics as core timeline persistence and makes retries, diagnostics, and resume behavior harder to reason about.

**Decision:**

Unavailable enrichment is represented as degraded mode when the main export can still complete. The exporter and related diagnostics distinguish between a valid base timeline with missing optional data and a genuine export failure.

**Rejected:**

- Failing fast for every unavailable provider datum.
- Silently skipping enrichment without signaling the outcome to the caller.
- Treating degraded exports as indistinguishable from fully enriched exports.
- Encoding degraded state only in logs with no effect on exporter outcome metadata.

**Consequences:**

Export becomes more resilient and resume behavior becomes easier to reason about under partial host failure. The tradeoff is that consumers, tests, and diagnostics have to understand one more success state beyond simple pass/fail.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](022-skip-unavailable-github-pr-data-without-failing-export.md) | [Next](033-support-provider-aware-login-flows.md)
