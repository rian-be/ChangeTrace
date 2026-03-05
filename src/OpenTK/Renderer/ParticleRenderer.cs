using ChangeTrace.Rendering.Snapshots;
using ChangeTrace.Rendering.States;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace ChangeTrace.OpenTK.Renderer;

/// <summary>
/// SRP: draws particles as GL_POINTS with a soft-circle fragment shader.
/// Single draw call — all particles uploaded as one VBO per frame.
/// </summary>
internal sealed class ParticleRenderer : IDisposable
{
    private const int MaxParticles  = 8192;
    private const int FloatsPerPart = 7;   // pos(2) color(4) size(1)

    private readonly int _vao;
    private readonly GpuBuffer<float> _vbo;
    private readonly float[] _data = new float[MaxParticles * FloatsPerPart];

    internal ParticleRenderer()
    {
        _vao = GL.GenVertexArray();
        _vbo = new GpuBuffer<float>();

        GL.BindVertexArray(_vao);
        _vbo.UploadEmpty(MaxParticles * FloatsPerPart);

        int stride = FloatsPerPart * sizeof(float);
        // aPos
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
        // aColor
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, stride, 2 * sizeof(float));
        // aSize
        GL.EnableVertexAttribArray(2);
        GL.VertexAttribPointer(2, 1, VertexAttribPointerType.Float, false, stride, 6 * sizeof(float));

        GL.BindVertexArray(0);
    }

    internal void Draw(IReadOnlyList<ParticleSnapshot> particles,
        Shaders.ShaderProgram shader, Matrix3 viewProj)
    {
        int count = Math.Min(particles.Count, MaxParticles);
        if (count == 0) return;

        for (int i = 0; i < count; i++)
        {
            var p = particles[i];
            int b = i * FloatsPerPart;
            _data[b]   = p.Position.X;
            _data[b+1] = p.Position.Y;
            _data[b+2] = p.Color.X;  // r
            _data[b+3] = p.Color.Y;  // g
            _data[b+4] = p.Color.Z;  // b
            _data[b+5] = p.Color.W * p.Alpha; // alpha combined with snapshot alpha
            _data[b+6] = p.Size;
        }

        _vbo.Bind();
        GL.BufferSubData(BufferTarget.ArrayBuffer, nint.Zero, count * FloatsPerPart * sizeof(float), _data);

        GL.Enable(EnableCap.ProgramPointSize);

        shader.Use();
        shader.Set("uViewProj", viewProj);

        GL.BindVertexArray(_vao);
        GL.DrawArrays(PrimitiveType.Points, 0, count);
        GL.BindVertexArray(0);

        GL.Disable(EnableCap.ProgramPointSize);
    }

    private static void UnpackColor(uint rgb, out float r, out float g, out float b)
    {
        r = ((rgb >> 16) & 0xFF) / 255f;
        g = ((rgb >>  8) & 0xFF) / 255f;
        b = ( rgb        & 0xFF) / 255f;
    }

    public void Dispose()
    {
        GL.DeleteVertexArray(_vao);
        _vbo.Dispose();
    }
}