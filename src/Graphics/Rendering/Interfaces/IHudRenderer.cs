using ChangeTrace.Rendering.States;
using OpenTK.Mathematics;

namespace ChangeTrace.Graphics.Rendering.Interfaces;

/// <summary>
/// Renders HUD overlays in screen space.
/// </summary>
internal interface IHudRenderer
{
    /// <summary>
    /// Draws the HUD for the current render state.
    /// </summary>
    void Draw(RenderState state, Matrix3 screenMatrix, float viewW, float viewH);
}