[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](013-keep-error-logs-detailed-for-failures.md)

## [026] Use Concise Warning Logs For Expected Degradation

*2026-06* | Status: accepted

**Context:**

These decisions distinguish hard failures from situations where the system continues with reduced data or functionality. This matters especially in provider-aware export, where incomplete data does not always mean the whole operation failed.

**Problem:**

Not every provider-API problem is an application failure.
Logging in this system is not only for debugging failures, but also for communicating to the operator the difference between a hard error and controlled degradation.

**Decision:**

For expected degradation, warning logs stay concise, while full exception detail is reserved for serious errors.
This decision separates logging policy for expected situations from logging policy for actual failure paths so the CLI stays readable without losing diagnostic information.

**Rejected:**

- Printing a full stack trace for every warning.
- Ciche pomijanie degradacji.
- One logging policy in which warnings, degraded mode, and failures are not distinguished from each other.
- Leaving success or export-failure semantics to implicit interpretation instead of explicit logging rules.

**Consequences:**

CLI is clearer, but diagnostics must nadal wskazywac co zostalo pominiete.
In practice, it is necessary to keep a clear threshold between warnings and errors, because that determines whether an export is treated as failed or simply incomplete.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](013-keep-error-logs-detailed-for-failures.md)
