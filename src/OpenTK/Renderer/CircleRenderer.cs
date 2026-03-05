using ChangeTrace.Rendering.Snapshots;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace ChangeTrace.OpenTK.Renderer;

/// <summary>
/// SRP: draws all <see cref="NodeSnapshot"/>s as SDF circles using instanced rendering.
/// One draw call for all nodes regardless of count.
/// </summary>
internal sealed class CircleRenderer : IDisposable
{
    private static readonly float[] QuadVerts = [-1,-1, 1,-1, -1,1, 1,-1, 1,1, -1,1];

    private readonly int _vao;
    private readonly GpuBuffer<float> _quadVbo;
    private readonly GpuBuffer<float> _instanceVbo;

    private const int InstanceStride = 8;   // cx cy r R G B A glow
    private const int MaxInstances   = 4096;
    private readonly float[] _instanceData = new float[MaxInstances * InstanceStride];

    internal CircleRenderer()
    {
        _vao         = GL.GenVertexArray();
        _quadVbo     = new GpuBuffer<float>();
        _instanceVbo = new GpuBuffer<float>();

        GL.BindVertexArray(_vao);

        _quadVbo.Bind();
        _quadVbo.Upload(QuadVerts.AsSpan(), BufferUsageHint.StaticDraw);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 8, 0);

        _instanceVbo.Bind();
        _instanceVbo.UploadEmpty(MaxInstances * InstanceStride);

        int stride = InstanceStride * sizeof(float);

        GL.EnableVertexAttribArray(1); // aCenter
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 0);
        GL.VertexAttribDivisor(1, 1);

        GL.EnableVertexAttribArray(2); // aRadius
        GL.VertexAttribPointer(2, 1, VertexAttribPointerType.Float, false, stride, 2 * sizeof(float));
        GL.VertexAttribDivisor(2, 1);

        GL.EnableVertexAttribArray(3); // aColor
        GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
        GL.VertexAttribDivisor(3, 1);

        GL.EnableVertexAttribArray(4); // aGlow
        GL.VertexAttribPointer(4, 1, VertexAttribPointerType.Float, false, stride, 7 * sizeof(float));
        GL.VertexAttribDivisor(4, 1);

        GL.BindVertexArray(0);
    }

    internal void Draw(IReadOnlyList<NodeSnapshot> nodes, Shaders.ShaderProgram shader, Matrix3 viewProj)
    {
        int count = Math.Min(nodes.Count, MaxInstances);
        if (count == 0) return;

        for (int i = 0; i < count; i++)
        {
            var n    = nodes[i];
            int base_ = i * InstanceStride;
            _instanceData[base_ + 0] = n.Position.X;
            _instanceData[base_ + 1] = n.Position.Y;
            _instanceData[base_ + 2] = n.Radius;
            _instanceData[base_ + 3] = n.Color.X; // r
            _instanceData[base_ + 4] = n.Color.Y; // g
            _instanceData[base_ + 5] = n.Color.Z; // b
            _instanceData[base_ + 6] = n.Color.W; // a
            _instanceData[base_ + 7] = n.Glow;
        }

        _instanceVbo.Bind();
        GL.BufferSubData(BufferTarget.ArrayBuffer, nint.Zero, count * InstanceStride * sizeof(float), _instanceData);

        shader.Use();
        shader.Set("uViewProj", viewProj);

        GL.BindVertexArray(_vao);
        GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 6, count);
        GL.BindVertexArray(0);
    }

    internal void DrawBloom(IReadOnlyList<NodeSnapshot> nodes, Shaders.ShaderProgram shader, Matrix3 viewProj)
    {
        var glowing = nodes.Where(n => n.Glow > 0.05f).ToList();
        if (glowing.Count == 0) return;

        var inflated = glowing.Select(n => n with { Radius = n.Radius * 2.5f, Glow = n.Glow * 0.4f }).ToList();
        Draw(inflated, shader, viewProj);
    }

    public void Dispose()
    {
        GL.DeleteVertexArray(_vao);
        _quadVbo.Dispose();
        _instanceVbo.Dispose();
    }
}