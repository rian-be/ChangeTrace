using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core.Diagnostics;
using ChangeTrace.Rendering.Camera;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Layout.Hive;
using ChangeTrace.Rendering.Layout.Hive.Core;
using ChangeTrace.Rendering.Processors.Handlers;
using ChangeTrace.Rendering.Translators;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Rendering.Factory;

/// <summary>
/// Default factory responsible for creating core rendering systems.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class DefaultRenderSystemFactory(
    IServiceProvider services)
    : IRenderSystemFactory
{
    /// <summary>
    /// Creates rendering systems and assembles the rendering pipeline.
    /// </summary>
    public (
        ISceneGraph scene,
        IAnimationSystem anim,
        Camera.Camera camera,
        ICameraController cameraCtrl,
        IRenderStateAssembler assembler,
        IEnumerable<IRenderCommandHandler> handlers,
        ILayoutEngine layout,
        ITranslationPipeline translation,
        IRenderOutput renderer)
        Create()
    {
        ISceneGraph scene =
            services.GetRequiredService<ISceneGraph>();

        IAnimationSystem anim =
            services.GetRequiredService<IAnimationSystem>();

        IRenderStateAssembler assembler =
            services.GetRequiredService<IRenderStateAssembler>();

        Camera.Camera camera =
            services.GetRequiredService<Camera.Camera>();

        camera.Zoom = 2.2f;

        ICameraController cameraCtrl =
            new CameraController(camera)
            {
                Mode = CameraFollowMode.FollowAverage
            };

        IDiagnosticsProvider diag =
            services.GetRequiredService<IDiagnosticsProvider>();

        IRenderCommandHandler[] handlers =
        [
            new BranchLabelHandler(scene),
            new FileNodeHandler(
                scene,
                anim,
                diag),
            new MoveActorHandler(
                scene,
                anim,
                assembler),
            new EdgeHandler(scene),
            new BundledEdgeHandler(scene),
            new ParticleBurstHandler(
                scene,
                anim),
            new PullRequestBadgeHandler(
                scene,
                anim)
        ];

        ILayoutEngine layout =
            new HiveLayout();

        ITranslationPipeline translation =
            TranslationPipeline.Default();

        IRenderOutput renderer =
            services.GetRequiredService<IRenderOutput>();

        return (
            scene,
            anim,
            camera,
            cameraCtrl,
            assembler,
            handlers,
            layout,
            translation,
            renderer);
    }
}