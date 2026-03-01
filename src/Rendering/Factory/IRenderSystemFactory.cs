using ChangeTrace.Rendering.Interfaces;

namespace ChangeTrace.Rendering.Factory;

/// <summary>
/// Factory interface for creating all core rendering subsystems.
/// </summary>
/// <remarks>
/// Provides single method to instantiate scene graph, animation system, camera,
/// camera controller, layout engine, translation pipeline, render state assembler,
/// command handlers, and output renderer in one tuple.
/// </remarks>
internal interface IRenderSystemFactory
{
    /// <summary>
    /// Creates complete set of rendering systems.
    /// </summary>
    /// <returns>
    /// Tuple containing:
    /// <list type="bullet">
    /// <item><see cref="ISceneGraph"/> scene graph instance.</item>
    /// <item><see cref="IAnimationSystem"/> animation system.</item>
    /// <item><see cref="Camera.Camera"/> camera instance.</item>
    /// <item><see cref="ICameraController"/> camera controller.</item>
    /// <item><see cref="IRenderStateAssembler"/> render state assembler.</item>
    /// <item><see cref="IRenderCommandHandler"/> collection of command handlers.</item>
    /// <item><see cref="ILayoutEngine"/> layout engine.</item>
    /// <item><see cref="ITranslationPipeline"/> translation pipeline.</item>
    /// <item><see cref="IRenderOutput"/> output renderer.</item>
    /// </list>
    /// </returns>
    (ISceneGraph scene,
        IAnimationSystem anim,
        Camera.Camera camera,
        ICameraController cameraCtrl,
        IRenderStateAssembler assembler,
        IEnumerable<IRenderCommandHandler> handlers,
        ILayoutEngine layout,
        ITranslationPipeline translation,
        IRenderOutput renderer)
        Create();
}