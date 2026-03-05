using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace ChangeTrace.OpenTK.Shaders;

/// <summary>
/// SRP: owns one GL shader program — compile, link, uniform cache, bind/unbind.
/// </summary>
internal sealed class ShaderProgram : IDisposable
{
    private readonly int                     _handle;
    private readonly Dictionary<string, int> _uniformCache = new();

    internal ShaderProgram(string vertSrc, string fragSrc)
    {
        int vert = Compile(ShaderType.VertexShader,   vertSrc);
        int frag = Compile(ShaderType.FragmentShader, fragSrc);

        _handle = GL.CreateProgram();
        GL.AttachShader(_handle, vert);
        GL.AttachShader(_handle, frag);
        GL.LinkProgram(_handle);

        GL.GetProgram(_handle, GetProgramParameterName.LinkStatus, out int status);
        if (status == 0)
            throw new InvalidOperationException(GL.GetProgramInfoLog(_handle));

        GL.DetachShader(_handle, vert);
        GL.DetachShader(_handle, frag);
        GL.DeleteShader(vert);
        GL.DeleteShader(frag);
    }

    internal void Use() => GL.UseProgram(_handle);

    // ------------------------------------------------------------------
    // Uniforms
    // ------------------------------------------------------------------

    internal void Set(string name, Matrix3 m) =>
        GL.UniformMatrix3(Loc(name), true, ref m);   // true = transpose: OpenTK row-major → OpenGL column-major

    internal void Set(string name, Vector2 v) =>
        GL.Uniform2(Loc(name), v);

    internal void Set(string name, Vector4 v) =>
        GL.Uniform4(Loc(name), v);

    internal void Set(string name, int i) =>
        GL.Uniform1(Loc(name), i);

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private int Loc(string name)
    {
        if (_uniformCache.TryGetValue(name, out int loc)) return loc;
        loc = GL.GetUniformLocation(_handle, name);
        _uniformCache[name] = loc;
        return loc;
    }

    private static int Compile(ShaderType type, string src)
    {
        int shader = GL.CreateShader(type);
        GL.ShaderSource(shader, src);
        GL.CompileShader(shader);
        GL.GetShader(shader, ShaderParameter.CompileStatus, out int status);
        if (status == 0)
            throw new InvalidOperationException(GL.GetShaderInfoLog(shader));
        return shader;
    }

    public void Dispose() => GL.DeleteProgram(_handle);
}