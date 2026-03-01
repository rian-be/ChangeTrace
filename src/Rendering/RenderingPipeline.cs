using ChangeTrace.Core.Events;
using ChangeTrace.Player.Enums;
using ChangeTrace.Player.Interfaces;
using ChangeTrace.Rendering.Commands;
using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Processors;
using ChangeTrace.Rendering.States;

namespace ChangeTrace.Rendering;

/// <summary>
/// Connects <see cref="ITimelinePlayer"/> to rendering system.
/// </summary>
/// <remarks>
/// Listens to timeline events, translates them into <see cref="RenderCommand"/> instances,
/// dispatches them to <see cref="IRenderCommandHandler"/>s, updates scene layout and animations,
/// applies camera control, and submits <see cref="RenderState"/> snapshots to output.
/// </remarks>
internal sealed class RenderingPipeline : IRenderingPipeline
{
    private readonly ISceneGraph _scene;
    private readonly IAnimationSystem _anim;
    private readonly Camera.Camera _camera;
    private readonly ICameraController _cameraCtrl;
    private readonly ILayoutEngine _layout;
    private readonly ITranslationPipeline _translation;
    private readonly SceneCommandDispatcher _dispatcher;
    private readonly ActorDecaySystem _actorDecay;
    private readonly IRenderStateAssembler _assembler;
    private readonly IRenderOutput _output;

    private readonly Vec2 _viewportSize;
    private double _lastWallTime;
    
    public ITimelinePlayer Player { get; }

    /// <summary>
    /// Creates new rendering pipeline instance.
    /// </summary>
    internal RenderingPipeline(
        ITimelinePlayer player,
        IRenderOutput output,
        ILayoutEngine layout,
        Camera.Camera camera,
        ICameraController cameraCtrl,
        ISceneGraph scene,
        IAnimationSystem anim,
        ITranslationPipeline translation,
        IRenderStateAssembler assembler,
        IEnumerable<IRenderCommandHandler> handlers,
        Vec2 viewportSize)
    {
        Player = player;
        _output = output;
        _layout = layout;
        _camera = camera;
        _cameraCtrl = cameraCtrl;
        _scene = scene;
        _anim = anim;
        _translation = translation;
        _assembler = assembler;
        _viewportSize = viewportSize;
        _dispatcher = new SceneCommandDispatcher(handlers);
        _actorDecay = new ActorDecaySystem(_scene);

        Player.OnEvent += OnEvent;
        Player.OnProgress += OnProgress;
        Player.OnStateChanged += OnStateChanged;
    }

    /// <summary>
    /// Starts timeline playback.
    /// </summary>
    public void Start() => Player.Play();

    /// <summary>
    /// Stops timeline playback and clears scene state, animations, and assembler counters.
    /// </summary>
    public void Stop()
    {
        Player.Stop();
        _scene.Clear();
        _anim.Clear();
        _assembler.Reset();
    }

    /// <summary>
    /// Handles timeline <see cref="TraceEvent"/> by translating it into render commands
    /// and dispatching them.
    /// </summary>
    /// <param name="evt">Timeline event to process.</param>
    private void OnEvent(TraceEvent evt)
    {
        var commands = _translation.Translate(evt);
        foreach (var cmd in commands)
            _dispatcher.Dispatch(cmd, evt.Timestamp.UnixSeconds);
    }

    /// <summary>
    /// Updates scene layout, animations, edges, actor decay, and camera for current progress.
    /// Submits assembled <see cref="RenderState"/> to output.
    /// </summary>
    /// <param name="progress">Current progress of timeline playback (0–1).</param>
    private void OnProgress(double progress)
    {
        var diag = Player.GetDiagnostics();
        var dt = (float)Math.Max(0, diag.WallElapsedSeconds - _lastWallTime);
        _lastWallTime = diag.WallElapsedSeconds;

        _layout.Step(_scene.Nodes, dt);
        _anim.Tick(dt);
        _scene.TickEdges(diag.PositionSeconds, decayRate: 1f);
        _actorDecay.Tick(diag.PositionSeconds);
        _cameraCtrl.Tick(_scene, dt, _viewportSize);

        var state = _assembler.Assemble(
            diag.PositionSeconds, dt, _scene, _anim, _camera, diag);

        _output.Submit(state);
    }

    /// <summary>
    /// Clears scene and resets assembler/animation state when player becomes idle.
    /// </summary>
    /// <param name="state">New player state.</param>
    private void OnStateChanged(PlayerState state)
    {
        if (state != PlayerState.Idle) return;
        _scene.Clear();
        _anim.Clear();
        _assembler.Reset();
    }
    
    public void Dispose()
    {
        Player.OnEvent -= OnEvent;
        Player.OnProgress -= OnProgress;
        Player.OnStateChanged -= OnStateChanged;
    }
}