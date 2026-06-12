[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](052-use-render-commands-as-visual-intent.md) | [Next](056-keep-animation-as-a-subsystem.md)

## [055] Order Event Translators By Explicit Priority

*2026-06* | Status: accepted

**Context:**

`TranslationPipeline` maintains a list of translators with explicit execution priority. Translators are registered separately, filtered by event type, and executed in a defined order instead of relying on accidental registration order or DI behavior.

**Problem:**

There can be more than one translator for a given event class, and the order of emitted commands matters for scene semantics. Without explicit ordering, translators become unstable and the resulting scene depends on initialization details instead of a declarative contract.

**Decision:**

The translation pipeline runs translators according to explicit priority while preserving `EventType` and `CanHandle` filtering. Translation order is treated as part of the rendering contract rather than an initialization detail.

**Rejected:**

- Relying on DI container registration order.
- Jeden translator zawierajacy cala logike dla wszystkich semantycznych eventow.
- Dynamiczne sortowanie po nazwach typow albo innych posrednich heurystykach.
- Moving responsibility for command ordering into scene dispatchers.

**Consequences:**

Translator behavior becomes more predictable and new translators are easier to add without hidden side effects. The downside is that priorities must be maintained deliberately as part of the architecture rather than as incidental configuration.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](052-use-render-commands-as-visual-intent.md) | [Next](056-keep-animation-as-a-subsystem.md)
