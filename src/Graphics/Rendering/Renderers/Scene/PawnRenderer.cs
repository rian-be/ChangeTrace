using ChangeTrace.Graphics.Gpu.Buffers;
using ChangeTrace.Graphics.Gpu.Pipelines;
using ChangeTrace.Graphics.Shaders.Runtime;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace ChangeTrace.Graphics.Rendering.Renderers.Scene;

/// <summary>
/// Renders GPU driven pawn instances.
/// </summary>
internal sealed class PawnRenderer : IDisposable
{
    /// <summary>
    /// Unit quad used as per-instance pawn geometry.
    /// </summary>
    private static readonly float[] QuadVerts =
    [
        -1f, -1f,
         1f, -1f,
        -1f,  1f,

         1f, -1f,
         1f,  1f,
        -1f,  1f
    ];

    private readonly VertexArray _vao = new();
    private readonly VertexBuffer<float> _quadVbo = new();

    private bool _disposed;

    internal PawnRenderer()
    {
        InitializeGpu();
    }

    /// <summary>
    /// Draws visible pawns using GPU-generated indirect commands.
    /// </summary>
    internal void Draw(
        PawnGpuPipeline pipeline,
        ShaderProgram shader,
        Matrix3 viewProj)
    {
        if (_disposed)
            return;

        shader.Use();

        shader.Set(
            "uViewProj",
            viewProj);

        GL.BindBufferBase(
            BufferRangeTarget.ShaderStorageBuffer,
            0,
            pipeline.VisiblePawnBuffer);

        GL.BindBuffer(
            BufferTarget.DrawIndirectBuffer,
            pipeline.IndirectBuffer);

        _vao.Bind();

        _vao.DrawArraysIndirect(
            PrimitiveType.Triangles);

        VertexArray.Unbind();

        GL.BindBuffer(
            BufferTarget.DrawIndirectBuffer,
            0);
    }

    /// <summary>
    /// Initializes GPU buffers for pawn rendering.
    /// </summary>
    private void InitializeGpu()
    {
        _vao.Bind();

        _quadVbo.Upload(
            QuadVerts,
            BufferUsageHint.StaticDraw);

        _vao.BindVertexBuffer(
            _quadVbo);

        _vao.AttributePointer(
            index: 0,
            componentCount: 2,
            type: VertexAttribPointerType.Float,
            strideBytes: 2 * sizeof(float),
            offsetBytes: 0);

        VertexArray.Unbind();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _quadVbo.Dispose();
        _vao.Dispose();

        _disposed = true;
    }
}