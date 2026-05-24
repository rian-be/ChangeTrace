using System.Diagnostics;

namespace ChangeTrace.Graphics.Rendering.Runtime;

/// <summary>
/// Lightweight sequential frame graph / render graph.
/// </summary>
internal sealed class RenderFrameGraph
{
    private readonly List<RenderPass> _passes = new();

    /// <summary>
    /// Registered passes.
    /// </summary>
    public IReadOnlyList<RenderPass> Passes =>
        _passes;

    /// <summary>
    /// Adds render pass to the graph.
    /// </summary>
    public T Add<T>(
        T pass)
        where T : RenderPass
    {
        ArgumentNullException.ThrowIfNull(
            pass);

        _passes.Add(
            pass);

        return pass;
    }

    /// <summary>
    /// Removes render pass.
    /// </summary>
    public bool Remove(
        RenderPass pass)
    {
        return _passes.Remove(
            pass);
    }

    /// <summary>
    /// Clears all registered passes.
    /// </summary>
    public void Clear()
    {
        _passes.Clear();
    }

    /// <summary>
    /// Executes all enabled passes sequentially.
    /// </summary>
    public void Execute(
        in RenderFrameContext context)
    {
        foreach (RenderPass pass in _passes)
        {
            if (!pass.Enabled)
                continue;

            pass.Render(
                context);
        }
    }

    /// <summary>
    /// Executes all enabled passes with profiling callback.
    /// </summary>
    public void ExecuteProfiled(
        in RenderFrameContext context,
        Action<RenderPass, TimeSpan>? completed)
    {
        foreach (RenderPass pass in _passes)
        {
            if (!pass.Enabled)
                continue;

            long start =
                Stopwatch.GetTimestamp();

            pass.Render(
                context);

            long end =
                Stopwatch.GetTimestamp();

            completed?.Invoke(
                pass,
                Stopwatch.GetElapsedTime(
                    start,
                    end));
        }
    }
}