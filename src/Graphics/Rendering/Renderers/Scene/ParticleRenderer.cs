using ChangeTrace.Graphics.Gpu.Buffers;
using ChangeTrace.Graphics.Gpu.Pipelines;
using ChangeTrace.Graphics.Shaders.Runtime;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace ChangeTrace.Graphics.Rendering.Renderers.Scene;

/// <summary>
/// Renders GPU driven particle instances.
/// </summary>
internal sealed class ParticleRenderer : IDisposable
{
    private readonly VertexArray _vao = new();

    private bool _disposed;

    /// <summary>
    /// Draws visible particles using GPU-generated indirect commands.
    /// </summary>
    public void Draw(
        ShaderProgram shader,
        ParticleGpuPipeline pipeline,
        Matrix3 viewProj)
    {
        if (_disposed)
            return;

        shader.Use();

        shader.Set(
            "uViewProj",
            viewProj);

        GL.Enable(
            EnableCap.ProgramPointSize);

        _vao.Bind();

        GL.BindBufferBase(
            BufferRangeTarget.ShaderStorageBuffer,
            0,
            pipeline.VisibleParticleBuffer);

        GL.BindBuffer(
            BufferTarget.DrawIndirectBuffer,
            pipeline.IndirectBuffer);

        _vao.DrawArraysIndirect(
            PrimitiveType.Points);

        GL.BindBuffer(
            BufferTarget.DrawIndirectBuffer,
            0);

        VertexArray.Unbind();

        GL.Disable(
            EnableCap.ProgramPointSize);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _vao.Dispose();

        _disposed = true;
    }
}