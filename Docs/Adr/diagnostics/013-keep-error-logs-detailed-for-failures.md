[ADR Home](../README.md) | [Category Index](./README.md) | [Next](026-use-concise-warning-logs-for-expected-degradation.md)

## [013] Keep Error Logs Detailed For Failures

*2026-06* | Status: accepted

**Context:**

These decisions distinguish hard failures from situations where the system continues with reduced data or functionality. This matters especially in provider-aware export, where incomplete data does not always mean the whole operation failed.

**Problem:**

Real export and runtime failures require full diagnostic context.
Logging in this system is not only for debugging failures, but also for communicating to the operator the difference between a hard error and controlled degradation.

**Decision:**

Logging keeps detailed information for `Error` and `Critical` cases.
This decision separates logging policy for expected situations from logging policy for actual failure paths so the CLI stays readable without losing diagnostic information.

**Rejected:**

- The same short logs for every level.
- Hiding exceptions from the operator or developer.
- One logging policy in which warnings, degraded mode, and failures are not distinguished from each other.
- Leaving success or export-failure semantics to implicit interpretation instead of explicit logging rules.

**Consequences:**

Debugging serious failures remains possible, but failure must be distinguished from degraded mode.
In practice, it is necessary to keep a clear threshold between warnings and errors, because that determines whether an export is treated as failed or simply incomplete.

[ADR Home](../README.md) | [Category Index](./README.md) | [Next](026-use-concise-warning-logs-for-expected-degradation.md)
