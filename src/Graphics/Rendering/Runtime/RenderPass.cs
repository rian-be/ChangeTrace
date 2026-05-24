namespace ChangeTrace.Graphics.Rendering.Runtime;

/// <summary>
/// Base render pass abstraction.
/// </summary>
internal abstract class RenderPass
{
    /// <summary>
    /// Pass the display /debug name.
    /// </summary>
    public virtual string Name =>
        GetType().Name;

    /// <summary>
    /// Whether pass is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Executes render pass.
    /// </summary>
    public abstract void Render(
        in RenderFrameContext context);
}