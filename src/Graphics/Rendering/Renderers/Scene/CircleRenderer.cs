using ChangeTrace.Graphics.Gpu.Buffers;
using ChangeTrace.Graphics.Gpu.Pipelines;
using ChangeTrace.Graphics.Shaders.Runtime;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace ChangeTrace.Graphics.Rendering.Renderers.Scene;

/// <summary>
/// Renders GPU driven circle/node instances.
/// </summary>
internal sealed class CircleRenderer : IDisposable
{
    /// <summary>
    /// Fullscreen style quad used as per instance circle geometry.
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

    public CircleRenderer()
    {
        _vao.Bind();

        _vao.BindVertexBuffer(_quadVbo);

        _quadVbo.Upload(
            QuadVerts,
            BufferUsageHint.StaticDraw);

        _vao.AttributePointer(
            index: 0,
            componentCount: 2,
            type: VertexAttribPointerType.Float,
            strideBytes: 2 * sizeof(float),
            offsetBytes: 0);

        VertexArray.Unbind();
    }

    /// <summary>
    /// Draws visible circle instances using GPU-generated indirect commands.
    /// </summary>
    public void DrawGpu(
        CircleGpuPipeline gpu,
        ShaderProgram renderShader,
        Matrix3 viewProj)
    {
        renderShader.Use();
        renderShader.Set("uViewProj", viewProj);

        GL.BindBufferBase(
            BufferRangeTarget.ShaderStorageBuffer,
            1,
            gpu.VisibleInstanceBuffer);

        GL.BindBuffer(
            BufferTarget.DrawIndirectBuffer,
            gpu.IndirectBuffer);

        _vao.Bind();
        _vao.DrawArraysIndirect(PrimitiveType.Triangles);
        VertexArray.Unbind();

        GL.BindBuffer(
            BufferTarget.DrawIndirectBuffer,
            0);
    }

    public void Dispose()
    {
        _quadVbo.Dispose();
        _vao.Dispose();
    }
}