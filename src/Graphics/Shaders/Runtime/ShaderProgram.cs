using OpenTK.Graphics.OpenGL4;

namespace ChangeTrace.Graphics.Shaders.Runtime;

/// <summary>
/// Represents compiled OpenGL graphics shader program.
/// </summary>
internal sealed class ShaderProgram : GlProgram
{
    /// <summary>
    /// Creates a graphics shader program from vertex and fragment GLSL sources.
    /// </summary>
    internal ShaderProgram(
        string vertexSource,
        string fragmentSource)
        : base(
            ShaderCompiler.CreateProgram(
                (ShaderType.VertexShader, vertexSource),
                (ShaderType.FragmentShader, fragmentSource)))
    {
    }
}