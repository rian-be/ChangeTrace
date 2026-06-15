[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](077-keep-buffer-lifecycle-and-binding-logic-out-of-renderers-and-pipelines.md) | [Next](079-provide-runtime-fallbacks-for-text-and-icon-assets.md)

## [078] Use Typed GPU Buffer Contracts

*2026-05* | Status: accepted

**Context:**

GPU pipelines only behave correctly when CPU side layout and shader side layout agree exactly. In a runtime with multiple buffer kinds, indirect draw commands, and compute passes, untyped buffer payloads make those contracts too easy to break silently.

**Problem:**

CPU and GPU must agree on data layout, binding meaning, and element semantics. If those rules exist only in shader code or only in comments, buffer misuse and layout drift become difficult to detect before runtime breakage.

**Decision:**

GPU data is described through explicit typed buffer contracts and matching wrappers. Buffer payload types encode layout assumptions on the CPU side, and the runtime binds them through buffer types that correspond to real usage patterns.

**Rejected:**

- Encoding data layout only in the shader.
- Using untyped byte ranges everywhere in CPU side pipeline code.
- Treating binding semantics as free form integers passed between subsystems.
- Copying layout definitions independently into each renderer or compute pass.

**Consequences:**

GPU buffer usage becomes more explicit and easier to review against shader expectations. The tradeoff is that changing a GPU side layout now requires coordinated updates to typed contracts instead of a local shader edit.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](077-keep-buffer-lifecycle-and-binding-logic-out-of-renderers-and-pipelines.md) | [Next](079-provide-runtime-fallbacks-for-text-and-icon-assets.md)
