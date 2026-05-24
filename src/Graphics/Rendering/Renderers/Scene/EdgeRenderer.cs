using ChangeTrace.Graphics.Gpu.Buffers;
using ChangeTrace.Graphics.Gpu.Pipelines;
using ChangeTrace.Graphics.Shaders.Runtime;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace ChangeTrace.Graphics.Rendering.Renderers.Scene;

/// <summary>
/// Renders GPU driven edge instances.
/// </summary>
internal sealed class EdgeRenderer : IDisposable
{
    private readonly VertexArray _vao = new();

    private bool _disposed;

    /// <summary>
    /// Draws visible edges using GPU-generated indirect commands.
    /// </summary>
    public void Draw(
        EdgeGpuPipeline pipeline,
        ShaderProgram shader,
        Matrix3 viewProj)
    {
        if (_disposed)
            return;

        shader.Use();

        shader.Set(
            "uViewProj",
            viewProj);

        _vao.Bind();

        GL.BindBufferBase(
            BufferRangeTarget.ShaderStorageBuffer,
            0,
            pipeline.VisibleEdgeBuffer);

        GL.BindBuffer(
            BufferTarget.DrawIndirectBuffer,
            pipeline.IndirectBuffer);

        _vao.DrawArraysIndirect(
            PrimitiveType.Triangles);

        GL.BindBuffer(
            BufferTarget.DrawIndirectBuffer,
            0);

        VertexArray.Unbind();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _vao.Dispose();

        _disposed = true;
    }
}