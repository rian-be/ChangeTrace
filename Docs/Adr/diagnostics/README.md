[ADR Home](../README.md)

# diagnostics

This category describes observability rules such as logging semantics and controlled degradation.

## Scope

Look here for warning/error behavior, diagnostic intent, and the difference between hard failure and expected degraded execution.

## Responsibility Boundaries

Diagnostics describes runtime signaling and observability policy. It should not redefine domain or export flow semantics.

## How To Start Reading

Start here when changing logs, degradation policy, or runtime observability behavior.

## ADR List

| ADR | Title |
| --- | --- |
| [013-keep-error-logs-detailed-for-failures.md](./013-keep-error-logs-detailed-for-failures.md) | Keep Error Logs Detailed For Failures |
| [026-use-concise-warning-logs-for-expected-degradation.md](./026-use-concise-warning-logs-for-expected-degradation.md) | Use Concise Warning Logs For Expected Degradation |
