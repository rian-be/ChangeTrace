using ChangeTrace.Graphics.Rendering.Runtime;
using OpenTK.Mathematics;

namespace ChangeTrace.Graphics.Rendering.Passes;

/// <summary>
/// Renders the animated fullscreen background layer.
/// </summary>
internal sealed class BackgroundRenderPass : RenderPass
{
    /// <summary>
    /// Executes the background render pass.
    /// </summary>
    public override void Render(in RenderFrameContext context)
    {
        var shader = context.Resources.Shaders.Graphics("Background");

        shader.Use();

        // Global animation time.
        shader.Set("uTime", context.TotalTime);

        // Camera zoom used for parallax/grid scaling.
        shader.Set("uZoom", context.State.Camera.Zoom);

        // Camera world position.
        shader.Set("uCamPos", new Vector2(
                context.State.Camera.Position.X,
                context.State.Camera.Position.Y
            )
        );

        // Fullscreen background draw.
        context.Resources.Renderers.Background.Draw(shader);
    }
}