using ChangeTrace.Core;
using ChangeTrace.Player.Enums;
using ChangeTrace.Player.Factory;
using ChangeTrace.Rendering.Factory;

namespace ChangeTrace.Rendering;

/// <summary>
/// Composition root for setting up and running rendering pipeline with timeline player.
/// </summary>
/// <remarks>
/// This class is responsible for:
/// <list type="bullet">
/// <item>Normalizing timeline.</item>
/// <item>Creating timeline player via <see cref="ITimelinePlayerFactory"/>.</item>
/// <item>Creating rendering system (scene, animation, camera, handlers, layout, translation, renderer) via <see cref="IRenderSystemFactory"/>.</item>
/// <item>Wiring the rendering pipeline to player.</item>
/// <item>Starting playback and waiting until it finishes.</item>
/// </list>
/// </remarks>
internal static class CompositionRoot
{
    /// <summary>
    /// Runs rendering pipeline for given timeline using specified factories.
    /// </summary>
    /// <param name="timeline">The timeline to visualize.</param>
    /// <param name="playerFactory">Factory for creating timeline player.</param>
    /// <param name="renderFactory">Factory for creating rendering system components.</param>
    /// <returns>A task that completes when playback finishes.</returns>
    internal static async Task RunAsync(
        Timeline timeline,
        ITimelinePlayerFactory playerFactory,
        IRenderSystemFactory renderFactory)
    {
        timeline.Normalize();
        
        var player = playerFactory.Create(
            timeline,
            initialSpeed: 1.5,
            acceleration: 2.5);

        var (scene, anim, camera, cameraCtrl, assembler, handlers, layout, translation, renderer)
            = renderFactory.Create();

        using var pipeline = new RenderingPipeline(
            player,
            renderer,
            layout,
            camera,
            cameraCtrl,
            scene,
            anim,
            translation,
            assembler,
            handlers,
            new Vec2(1920f, 1080f));
        
        pipeline.Player.Mode = PlaybackMode.Once;
        pipeline.Player.TargetSpeed = 5.0;
        pipeline.Start();
        
        while (pipeline.Player.State != PlayerState.Finished)
            await Task.Delay(16);

        Console.WriteLine(pipeline.Player.GetDiagnostics());
    }
}