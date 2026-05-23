using ChangeTrace.Rendering.Interfaces;

namespace ChangeTrace.Rendering.Factory;

/// <summary>
/// Factory interface for creating core rendering subsystems.
/// </summary>
internal interface IRenderSystemFactory
{
    /// <summary>
    /// Creates rendering systems and assembles the rendering pipeline.
    /// </summary>
    (
        ISceneGraph scene,
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