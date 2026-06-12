[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](076-build-specialized-buffer-wrappers-on-top-of-a-generic-gpu-buffer.md) | [Next](078-use-typed-gpu-buffer-contracts.md)

## [077] Keep Buffer Lifecycle And Binding Logic Out Of Renderers And Pipelines

*2026-06* | Status: accepted

**Context:**

Classes in `ChangeTrace.Graphics.Gpu.Buffers` encapsulate operations such as `GenBuffer`, `BindBuffer`, `BufferData`, `BufferSubData`, `BindBase`, and `DeleteBuffer`. GPU pipelines and scene renderers use those wrappers, but do not manage the full lifecycle of raw buffer handles directly.

**Problem:**

A renderer or pipeline that both computes data, manages shaders, and manually handles raw buffer lifecycle quickly accumulates too many responsibilities. That increases the risk of OpenGL state errors, inconsistent binding, and hard-to-find resource leaks.

**Decision:**

Buffer lifecycle and basic binding operations remain encapsulated in a dedicated `Gpu.Buffers` layer. Renderers and pipelines work with wrappers that carry explicit semantics instead of directly managing handles and raw `GL.*Buffer*` calls.

**Rejected:**

- Creating and deleting buffer handles manually in each renderer.
- Duplicating `Bind` / `BufferData` / `Delete` sequences in compute pipelines and passes.
- Treating buffer lifecycle as a detail of one renderer instead of shared infrastructure.
- Hiding buffer semantics behind a generic helper without usage-specific types.

**Consequences:**

Execution becomes less vulnerable to inconsistent OpenGL state, and GPU resource management stays concentrated in one place. The downside is that buffer wrappers become a critical part of the runtime, so their failures have broader impact than a single renderer failure.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](076-build-specialized-buffer-wrappers-on-top-of-a-generic-gpu-buffer.md) | [Next](078-use-typed-gpu-buffer-contracts.md)
