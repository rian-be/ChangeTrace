[ADR Home](../README.md)

# aggregation

This category describes higher-level semantic event models built on top of TraceEvent.

## Scope

Look here for bundling, coupling, pull request semantics, and other aggregate event models that sit above raw trace events.

## Responsibility Boundaries

Aggregation enriches meaning before later layers consume it. It should not become a rendering or persistence layer by itself.

## How To Start Reading

Start here when changing semantic event derivation or higher-level event grouping.

## ADR List

| ADR | Title |
| --- | --- |
| [054-model-pull-requests-as-semantic-events.md](./054-model-pull-requests-as-semantic-events.md) | Model Pull Requests As Semantic Events |
| [062-aggregate-commit-bundles-explicitly.md](./062-aggregate-commit-bundles-explicitly.md) | Aggregate Commit Bundles Explicitly |
| [063-aggregate-file-coupling-explicitly.md](./063-aggregate-file-coupling-explicitly.md) | Aggregate File Coupling Explicitly |
| [064-use-semantic-aggregation-before-rendering.md](./064-use-semantic-aggregation-before-rendering.md) | Use Semantic Aggregation Before Rendering |
