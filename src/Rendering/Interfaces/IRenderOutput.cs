using ChangeTrace.Rendering.States;

namespace ChangeTrace.Rendering.Interfaces;

/// <summary>
/// Receives rendered scene snapshots.
/// </summary>
internal interface IRenderOutput
{
    /// <summary>
    /// Initializes rendering output resources.
    /// </summary>
    void Initialize(
        int width,
        int height);

    /// <summary>
    /// Resizes rendering output viewport.
    /// </summary>
    void Resize(
        int width,
        int height);

    /// <summary>
    /// Submits immutable render state snapshot.
    /// </summary>
    void Submit(RenderState state);

    /// <summary>
    /// Enables or disables beam rendering.
    /// </summary>
    static bool ShowBeams { get; set; } = true;
}