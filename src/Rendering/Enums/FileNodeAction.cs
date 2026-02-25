namespace ChangeTrace.Rendering.Enums;

/// <summary>
/// Represents actions that can be performed on file node in rendering.
/// </summary>
internal enum FileNodeAction
{
    /// <summary>
    /// Add or spawn new file node.
    /// </summary>
    Spawn,

    /// <summary>
    /// Animate or highlight an existing file node temporarily.
    /// </summary>
    Pulse,

    /// <summary>
    /// Remove an existing file node.
    /// </summary>
    Remove
}