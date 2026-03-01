using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Rendering.Animation;
using ChangeTrace.Rendering.Camera;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Layout;
using ChangeTrace.Rendering.Layout.Forces;
using ChangeTrace.Rendering.Layout.Integration;
using ChangeTrace.Rendering.Layout.Proximity;
using ChangeTrace.Rendering.Outputs;
using ChangeTrace.Rendering.Processors.Handlers;
using ChangeTrace.Rendering.Scene;
using ChangeTrace.Rendering.States;
using ChangeTrace.Rendering.Translators;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Rendering.Factory;

/// <summary>
/// Default factory for creating all core rendering systems.
/// </summary>
/// <remarks>
/// Uses DI container to resolve services and assembles scene graph, animation system,
/// camera, camera controller, layout engine, translation pipeline, render state assembler,
/// command handlers, and output renderer.
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class DefaultRenderSystemFactory(IServiceProvider services) : IRenderSystemFactory
{
    /// <summary>
    /// Creates complete set of rendering systems.
    /// </summary>
    /// <returns>
    /// Tuple containing:
    /// <list type="bullet">
    /// <item><see cref="ISceneGraph"/> scene graph instance.</item>
    /// <item><see cref="IAnimationSystem"/> animation system.</item>
    /// <item><see cref="Camera"/> camera instance.</item>
    /// <item><see cref="ICameraController"/> camera controller.</item>
    /// <item><see cref="IRenderStateAssembler"/> render state assembler.</item>
    /// <item><see cref="IRenderCommandHandler"/> collection of command handlers.</item>
    /// <item><see cref="ILayoutEngine"/> layout engine.</item>
    /// <item><see cref="ITranslationPipeline"/> translation pipeline.</item>
    /// <item><see cref="IRenderOutput"/> output renderer.</item>
    /// </list>
    /// </returns>
    public (ISceneGraph scene,
            IAnimationSystem anim,
            Camera.Camera camera,
            ICameraController cameraCtrl,
            IRenderStateAssembler assembler,
            IEnumerable<IRenderCommandHandler> handlers,
            ILayoutEngine layout,
            ITranslationPipeline translation,
            IRenderOutput renderer) Create()
    {
        var scene = services.GetRequiredService<ISceneGraph>();
        var anim = services.GetRequiredService<IAnimationSystem>();
        var assembler = services.GetRequiredService<IRenderStateAssembler>();
        var camera = services.GetRequiredService<Camera.Camera>();

        var cameraCtrl = new CameraController(camera)
        {
            Mode = CameraFollowMode.FollowAverage
        };
        
        var handlers = new IRenderCommandHandler[]
        {
            new MoveActorHandler(scene, anim, assembler),
            new FileNodeHandler(scene, anim),
            new EdgeHandler(scene),
            new ParticleBurstHandler(scene, anim),
            new BranchLabelHandler(scene),
            new PullRequestBadgeHandler(scene, anim)
        };
        
        var layout = new ForceDirectedLayout(
            new ForceDirectedCalculator(new DirectoryProximity()),
            new VelocityIntegrator()
        );
        
        var translation = TranslationPipeline.Default();
        var renderer = services.GetService<IRenderOutput>() ?? new DebugRenderOutput();

        return (scene, anim, camera, cameraCtrl, assembler, handlers, layout, translation, renderer);
    }
}