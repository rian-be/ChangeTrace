using ChangeTrace.Graphics.Gpu.Buffers;
using ChangeTrace.Graphics.Gpu.Contracts;
using OpenTK.Graphics.OpenGL4;

namespace ChangeTrace.Graphics.Gpu.Pipelines.Base;

/// <summary>
/// Base class for GPU-driven compute pipelines.
/// Handles GPU upload, visibility compaction and indirect draw generation.
/// </summary>
internal abstract class IndirectComputePipeline<TGpuItem> : IDisposable
    where TGpuItem : unmanaged
{
    /// <summary>
    /// Source items uploaded from CPU memory.
    /// </summary>
    protected readonly ShaderStorageBuffer<TGpuItem> InputStorageBuffer = new();

    /// <summary>
    /// GPU-compacted visible items.
    /// </summary>
    protected readonly ShaderStorageBuffer<TGpuItem> VisibleStorageBuffer = new();

    /// <summary>
    /// GPU-written visible instance counter.
    /// </summary>
    private readonly ShaderStorageBuffer<uint> _counterStorageBuffer = new();

    /// <summary>
    /// Indirect draw command buffer.
    /// </summary>
    protected readonly IndirectDrawBuffer<DrawArraysIndirectCommand> IndirectDrawBuffer = new();

    /// <summary>
    /// Vertex count rendered per instance.
    /// </summary>
    private uint _vertexCountPerInstance;

    /// <summary>
    /// Allocates GPU buffers required by the pipeline.
    /// </summary>
    protected void Initialize(
        int maxItems,
        uint vertexCountPerInstance,
        BufferUsageHint inputUsage = BufferUsageHint.DynamicDraw)
    {
        _vertexCountPerInstance = vertexCountPerInstance;

        InputStorageBuffer.UploadEmpty(maxItems, inputUsage);
        VisibleStorageBuffer.UploadEmpty(maxItems);
        _counterStorageBuffer.UploadEmpty(1);

        ResetIndirect();
    }

    /// <summary>
    /// Resets visible counts and indirect draw state.
    /// </summary>
    protected void ResetIndirect()
    {
        uint zero = 0;

        _counterStorageBuffer.SubData([zero]);

        IndirectDrawBuffer.UploadCommand(
            new DrawArraysIndirectCommand
            {
                Count = _vertexCountPerInstance,
                InstanceCount = 0,
                First = 0,
                BaseInstance = 0
            });

        const MemoryBarrierFlags barriers =
            (MemoryBarrierFlags)(
                MemoryBarrierFlags.CommandBarrierBit |
                MemoryBarrierFlags.ShaderStorageBarrierBit);

        GL.MemoryBarrier(barriers);
    }

    /// <summary>
    /// Binds shared storage buffers used by compute shaders.
    /// </summary>
    protected void BindCommonBuffers()
    {
        InputStorageBuffer.BindBase(0);
        VisibleStorageBuffer.BindBase(1);
        _counterStorageBuffer.BindBase(2);

        GL.BindBufferBase(
            BufferRangeTarget.ShaderStorageBuffer,
            3,
            IndirectDrawBuffer.Handle);
    }

    /// <summary>
    /// Dispatches compute workgroups using 256-thread groups.
    /// </summary>
    protected static void Dispatch256(int itemCount)
    {
        int groups = (itemCount + 255) / 256;

        GL.DispatchCompute(groups, 1, 1);

        const MemoryBarrierFlags barriers =
            (MemoryBarrierFlags)(
                MemoryBarrierFlags.CommandBarrierBit |
                MemoryBarrierFlags.ShaderStorageBarrierBit |
                MemoryBarrierFlags.VertexAttribArrayBarrierBit);

        GL.MemoryBarrier(barriers);
    }

    public virtual void Dispose()
    {
        InputStorageBuffer.Dispose();
        VisibleStorageBuffer.Dispose();
        _counterStorageBuffer.Dispose();
        IndirectDrawBuffer.Dispose();
    }
}