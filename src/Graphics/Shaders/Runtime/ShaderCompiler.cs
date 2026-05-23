using OpenTK.Graphics.OpenGL4;

namespace ChangeTrace.Graphics.Shaders.Runtime;

/// <summary>
/// Compiles and links OpenGL shader programs.
/// </summary>
internal static class ShaderCompiler
{
    /// <summary>
    /// Creates linked OpenGL program from shader stages.
    /// </summary>
    public static int CreateProgram(
        params (ShaderType Type, string Source)[] stages)
    {
        int program =
            GL.CreateProgram();

        int[] shaders =
            new int[stages.Length];

        try
        {
            for (int i = 0; i < stages.Length; i++)
            {
                shaders[i] =
                    Compile(
                        stages[i].Type,
                        stages[i].Source);

                GL.AttachShader(
                    program,
                    shaders[i]);
            }

            GL.LinkProgram(program);

            GL.GetProgram(
                program,
                GetProgramParameterName.LinkStatus,
                out int status);

            if (status == 0)
            {
                throw new InvalidOperationException(
                    GL.GetProgramInfoLog(program));
            }

            return program;
        }
        catch
        {
            GL.DeleteProgram(program);
            throw;
        }
        finally
        {
            foreach (int shader in shaders)
            {
                if (shader == 0)
                    continue;

                GL.DetachShader(
                    program,
                    shader);

                GL.DeleteShader(shader);
            }
        }
    }

    /// <summary>
    /// Compiles a single GLSL shader stage.
    /// </summary>
    private static int Compile(
        ShaderType type,
        string source)
    {
        int shader =
            GL.CreateShader(type);

        GL.ShaderSource(
            shader,
            source);

        GL.CompileShader(shader);

        GL.GetShader(
            shader,
            ShaderParameter.CompileStatus,
            out int status);

        if (status != 0)
            return shader;

        string error =
            GL.GetShaderInfoLog(shader);

        GL.DeleteShader(shader);

        throw new InvalidOperationException(error);
    }
}