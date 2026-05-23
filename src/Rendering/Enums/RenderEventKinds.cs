namespace ChangeTrace.Rendering.Enums;

/// <summary>
/// Bitmask describing renderable timeline event categories.
/// </summary>
[Flags]
internal enum RenderEventKinds
{
    /// <summary>
    /// No render event kinds enabled.
    /// </summary>
    None = 0,

    /// <summary>
    /// Commit events.
    /// </summary>
    Commit = 1 << 0,

    /// <summary>
    /// Branch creation or branch activity events.
    /// </summary>
    Branch = 1 << 1,

    /// <summary>
    /// Merge events.
    /// </summary>
    Merge = 1 << 2,

    /// <summary>
    /// File coupling or relationship events.
    /// </summary>
    FileCoupling = 1 << 3,

    /// <summary>
    /// All primary render event kinds.
    /// </summary>
    All = Commit | Branch | Merge
}