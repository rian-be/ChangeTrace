using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.States;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Graphics.Rendering;

/// <summary>
/// OpenTK based render output implementation.
/// Owns the rendering runtime lifecycle and forwards render events.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class OpenTkRenderOutput : IRenderOutput, IDisposable
{
    /// <summary>
    /// Global toggle for beam rendering effects.
    /// </summary>
    public static bool ShowBeams { get; set; } = true;

    private OpenTkRenderRuntime? _runtime;

    /// <summary>
    /// Controls label rendering visibility.
    /// </summary>
    internal bool ShowLabels
    {
        get => _runtime?.ShowLabels ?? true;
        set => _runtime?.ShowLabels = value;
    }

    /// <summary>
    /// Controls HUD rendering visibility.
    /// </summary>
    internal bool ShowHud
    {
        get => _runtime?.ShowHud ?? true;
        set => _runtime?.ShowHud = value;
    }

    /// <summary>
    /// Initializes the rendering runtime.
    /// </summary>
    public void Initialize(int viewportWidth, int viewportHeight)
    {
        _runtime =
            new OpenTkRenderRuntime();

        _runtime.Initialize(
            viewportWidth,
            viewportHeight);
    }

    /// <summary>
    /// Submits a render state for rendering.
    /// </summary>
    public void Submit(RenderState state) =>
        _runtime?.Render(
            state);

    /// <summary>
    /// Resizes the rendering viewport.
    /// </summary>
    public void Resize(int width, int height) =>
        _runtime?.Resize(
            width,
            height);

    /// <summary>
    /// Releases rendering resources.
    /// </summary>
    public void Dispose()
    {
        _runtime?.Dispose();
        _runtime = null;
    }
}