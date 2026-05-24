namespace ChangeTrace.Graphics.Shaders.Registry;

/// <summary>
/// Describes shader resource configuration used for runtime shader creation.
/// </summary>
/// <param name="Name">Logical shader identifier.</param>
/// <param name="Kind">Shader program type.</param>
/// <param name="VertPath">Path to vertex shader sources.</param>
/// <param name="FragPath">Path to fragment shader sources.</param>
/// <param name="ComputePath">Path to compute shader sources.</param>
internal readonly record struct ShaderDefinition(
    string Name,
    ShaderKind Kind,
    string? VertPath,
    string? FragPath,
    string? ComputePath);