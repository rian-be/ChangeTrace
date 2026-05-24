using ChangeTrace.Graphics.Rendering.Interfaces;
using ChangeTrace.Graphics.Rendering.Renderers.Hud;
using ChangeTrace.Graphics.Rendering.Runtime;

namespace ChangeTrace.Graphics.Rendering.Passes;

/// <summary>
/// Renders HUD, labels and pod hover cards.
/// </summary>
internal sealed class HudRenderPass(IHudRenderer hudRenderer, ILabelRenderer labelRenderer, PodHoverCardRenderer podHoverCardRenderer) : RenderPass
{
    /// <summary>
    /// Controls whether the main HUD overlay is rendered.
    /// </summary>
    public bool ShowHud { get; set; } = true;

    /// <summary>
    /// Controls whether scene labels are rendered.
    /// </summary>
    public bool ShowLabels { get; set; } = true;

    /// <summary>
    /// Executes the HUD render pass.
    /// </summary>
    public override void Render(in RenderFrameContext context)
    {
        if (ShowLabels)
        {
            // Hide the regular node hover label while pod hover is active.
            string? hoveredNodeForLabels =
                context.State.Hud.Interaction.HoveredPod != null
                    ? null
                    : context.State.Hud.Interaction.HoveredNodeId;

            labelRenderer.Draw(
                context.State.Scene,
                context.State.Camera,
                context.ScreenProjection,
                context.ViewportWidth,
                context.ViewportHeight,
                hoveredNodeForLabels,
                hoveredPod: null);
        }

        // Render pod hover card independently from main HUD visibility.
        podHoverCardRenderer.Draw(
            context.Resources,
            context.State,
            context.ViewProjection,
            context.ViewportWidth,
            context.ViewportHeight,
            context.TotalTime);

        if (!ShowHud)
            return;

        hudRenderer.Draw(
            context.State,
            context.ScreenProjection,
            context.ViewportWidth,
            context.ViewportHeight);
    }
}