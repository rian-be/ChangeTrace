[ADR Home](../README.md) | [Category Index](./README.md) | [Next](021-make-enrichment-optional-per-export.md)

## [020] Keep Provider Specific Enrichment Behind Interfaces

*2026-06* | Status: accepted

**Context:**

Pull requests, merge metadata, and host specific repository details come from APIs that differ materially across GitHub, GitLab, Codeberg, and custom providers. The export pipeline needs those enrichments, but the base timeline and exporter orchestration should not depend on one hosting API shape.

**Problem:**

If provider specific enrichment logic sits directly in the exporter, every host capability difference leaks into staging, retry behavior, diagnostics, and command handling. That makes the common export path harder to extend and turns host support into conditionals scattered across unrelated modules.

**Decision:**

Provider specific enrichment is implemented behind shared interfaces. The exporter depends on enrichment contracts and provider resolution, while each host integration owns its own API calls, capability rules, and mapping into the enrichment model.

**Rejected:**

- One GitHub centric enricher used as the default model for every provider.
- Putting provider conditions directly in `RepositoryExporter`.
- Flattening all providers to the same lowest common denominator at the cost of losing their real constraints.
- Spreading provider specific logic across the exporter, prompts, auth flows, and history reading layers.

**Consequences:**

Adding or changing provider integrations becomes more localized, and the exporter can stay focused on orchestration. The cost is that interface design and provider resolution become important contracts that have to stay aligned with host capability differences.

[ADR Home](../README.md) | [Category Index](./README.md) | [Next](021-make-enrichment-optional-per-export.md)
