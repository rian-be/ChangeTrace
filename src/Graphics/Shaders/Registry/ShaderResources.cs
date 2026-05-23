using ChangeTrace.Graphics.Shaders.Runtime;

namespace ChangeTrace.Graphics.Shaders.Registry;

/// <summary>
/// Runtime registry for compiled graphics and compute shaders.
/// </summary>
internal sealed class ShaderResources : IDisposable
{
    private readonly Dictionary<string, ShaderProgram> _graphics =
        new();

    private readonly Dictionary<string, ComputeShader> _compute =
        new();

    /// <summary>
    /// Loads and compiles all configured shader programs.
    /// </summary>
    public ShaderResources()
    {
        foreach (ShaderDefinition def in ShaderManifest.All)
        {
            if (def.Kind == ShaderKind.Graphics)
            {
                _graphics.Add(
                    def.Name,
                    new ShaderProgram(
                        ShaderSource.Load(def.VertPath!),
                        ShaderSource.Load(def.FragPath!)));

                continue;
            }

            _compute.Add(
                def.Name,
                new ComputeShader(
                    ShaderSource.Load(def.ComputePath!)));
        }
    }

    /// <summary>
    /// Resolves compiled graphics shader by name.
    /// </summary>
    public ShaderProgram Graphics(string name)
    {
        return _graphics[name];
    }

    /// <summary>
    /// Resolves compiled compute shader by name.
    /// </summary>
    public ComputeShader Compute(string name)
    {
        return _compute[name];
    }

    public void Dispose()
    {
        foreach (ShaderProgram shader in _graphics.Values)
            shader.Dispose();

        foreach (ComputeShader shader in _compute.Values)
            shader.Dispose();
    }
}