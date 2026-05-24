using System.Collections.Concurrent;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace ChangeTrace.Graphics.Shaders.Runtime;

/// <summary>
/// Base wrapper around an OpenGL shader program.
/// </summary>
internal abstract class GlProgram(int handle) : IDisposable
{
    private readonly ConcurrentDictionary<string, int> _uniformCache =
        new();

    /// <summary>
    /// OpenGL shader program handle.
    /// </summary>
    private int Handle { get; } = handle;

    /// <summary>
    /// Binds the shader program.
    /// </summary>
    public void Use() =>
        GL.UseProgram(Handle);

    /// <summary>
    /// Uploads integer uniform value.
    /// </summary>
    public void Set(string name, int value) =>
        GL.Uniform1(Loc(name), value);

    /// <summary>
    /// Uploads floating-point uniform value.
    /// </summary>
    public void Set(string name, float value) =>
        GL.Uniform1(Loc(name), value);

    /// <summary>
    /// Uploads two-component vector uniform value.
    /// </summary>
    public void Set(string name, Vector2 value) =>
        GL.Uniform2(Loc(name), value);

    /// <summary>
    /// Uploads four-component vector uniform value.
    /// </summary>
    public void Set(string name, Vector4 value) =>
        GL.Uniform4(Loc(name), value);

    /// <summary>
    /// Uploads 3x3 matrix uniform value.
    /// </summary>
    public void Set(string name, Matrix3 value) =>
        GL.UniformMatrix3(Loc(name), false, ref value);

    /// <summary>
    /// Resolves cached uniform location.
    /// </summary>
    private int Loc(string name) =>
        _uniformCache.GetOrAdd(
            name,
            static (n, h) => GL.GetUniformLocation(h, n),
            Handle);

    public void Dispose() =>
        GL.DeleteProgram(Handle);
}