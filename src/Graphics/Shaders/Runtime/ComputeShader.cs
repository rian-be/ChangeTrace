using OpenTK.Graphics.OpenGL4;

namespace ChangeTrace.Graphics.Shaders.Runtime;

/// <summary>
/// Represents compiled OpenGL compute shader program.
/// </summary>
internal sealed class ComputeShader : GlProgram
{
    /// <summary>
    /// Creates compute shader program from GLSL source code.
    /// </summary>
    /// <param name="source">Compute shader GLSL source.</param>
    internal ComputeShader(string source)
        : base(
            ShaderCompiler.CreateProgram(
                (ShaderType.ComputeShader, source)))
    {
    }
}