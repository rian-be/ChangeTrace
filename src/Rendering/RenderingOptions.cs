using ChangeTrace.Rendering.Enums;

namespace ChangeTrace.Rendering;

/// <summary>
/// Configuration options controlling which rendering event categories are enabled.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Uses <see cref="RenderEventKinds"/> flags to enable or disable specific event types.</item>
/// <item>Provides convenience properties for checking if a specific event category should be rendered.</item>
/// <item>Defaults to rendering all event kinds.</item>
/// </list>
/// </remarks>
internal sealed class RenderingOptions
{
    /// <summary>
    /// Gets or sets the set of enabled rendering event kinds.
    /// </summary>
    public RenderEventKinds EnabledEvents { get; set; } = RenderEventKinds.All;

    /// <summary>
    /// Gets value indicating whether commit events should be rendered.
    /// </summary>
    public bool RenderCommits => EnabledEvents.HasFlag(RenderEventKinds.Commit);

    /// <summary>
    /// Gets value indicating whether branch events should be rendered.
    /// </summary>
    public bool RenderBranches => EnabledEvents.HasFlag(RenderEventKinds.Branch);

    /// <summary>
    /// Gets a value indicating whether merge events should be rendered.
    /// </summary>
    public bool RenderMerges => EnabledEvents.HasFlag(RenderEventKinds.Merge);

    /// <summary>
    /// Gets a value indicating whether file coupling events should be rendered.
    /// </summary>
    public bool RenderFileCoupling => EnabledEvents.HasFlag(RenderEventKinds.FileCoupling);
}