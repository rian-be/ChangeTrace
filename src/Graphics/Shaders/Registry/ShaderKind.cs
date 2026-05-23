namespace ChangeTrace.Graphics.Shaders.Registry;

/// <summary>
/// Defines runtime shader program category.
/// </summary>
internal enum ShaderKind
{
    /// <summary>
    /// Traditional graphics pipeline shader program.
    /// </summary>
    Graphics,

    /// <summary>
    /// Compute shader program.
    /// </summary>
    Compute
}