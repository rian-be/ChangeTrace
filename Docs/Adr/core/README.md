[ADR Home](../README.md)

# core

This category describes the domain core: events, timelines, value objects, filters, and Core responsibility boundaries.

## Scope

Look here for questions about what the system considers to be primary data and how the shared model is defined for export, player, aggregation, and rendering.

## Responsibility Boundaries

Core should not depend on a specific provider, storage backend, or GPU runtime. It is the meaning of the data, not an integration layer.

## How To Start Reading

Start here when changing TraceEvent, timelines, domain identifiers, or filtering rules.

## ADR List

| ADR | Title |
| --- | --- |
| [003-keep-filtering-behind-specifications.md](./003-keep-filtering-behind-specifications.md) | Keep Filtering Behind Specifications |
| [004-keep-the-domain-model-independent-from-git-providers.md](./004-keep-the-domain-model-independent-from-git-providers.md) | Keep The Domain Model Independent From Git Providers |
| [005-represent-identifiers-as-value-objects.md](./005-represent-identifiers-as-value-objects.md) | Represent Identifiers As Value Objects |
| [006-track-branch-lifecycle-explicitly.md](./006-track-branch-lifecycle-explicitly.md) | Track Branch Lifecycle Explicitly |
| [007-use-result-types-for-recoverable-domain-operations.md](./007-use-result-types-for-recoverable-domain-operations.md) | Use Result Types For Recoverable Domain Operations |
| [008-keep-unstable-query-apis-internal.md](./008-keep-unstable-query-apis-internal.md) | Keep Unstable Query Apis Internal |
| [009-build-timelines-through-a-dedicated-builder.md](./009-build-timelines-through-a-dedicated-builder.md) | Build Timelines Through A Dedicated Builder |
| [010-model-repository-activity-as-trace-events.md](./010-model-repository-activity-as-trace-events.md) | Model Repository Activity As Trace Events |
| [011-optimize-timeline-builder-hot-paths.md](./011-optimize-timeline-builder-hot-paths.md) | Optimize Timeline Builder Hot Paths |
| [065-use-a-reusable-aggregation-engine-for-streaming-semantic-processing.md](./065-use-a-reusable-aggregation-engine-for-streaming-semantic-processing.md) | Use A Reusable Aggregation Engine For Streaming Semantic Processing |
