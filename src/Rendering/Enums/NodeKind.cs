namespace ChangeTrace.Rendering.Enums;

/// <summary>
/// Defines semantic role of node within scene graph.
/// </summary>
/// <remarks>
/// The <see cref="NodeKind"/> influences:
/// <list type="bullet">
/// <item>Visual representation (e.g., radius, styling).</item>
/// <item>Layout behavior in force directed simulation.</item>
/// <item>Hierarchy semantics within rendered graph.</item>
/// </list>
/// </remarks>
internal enum NodeKind
{
    /// <summary>
    /// Represents file level node.
    /// Typically, smallest visual element.
    /// </summary>
    File,

    /// <summary>
    /// Represents a branch node.
    /// Groups related file nodes.
    /// </summary>
    Branch,

    /// <summary>
    /// Represents root node of scene.
    /// Usually largest and most central element.
    /// </summary>
    Root
}