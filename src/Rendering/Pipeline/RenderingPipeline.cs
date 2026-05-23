using ChangeTrace.Core.Diagnostics;
using ChangeTrace.Core.Events;
using ChangeTrace.Core.Events.Semantic;
using ChangeTrace.Player.Enums;
using ChangeTrace.Player.Interfaces;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Processors;

namespace ChangeTrace.Rendering.Pipeline;

/// <summary>
/// Coordinates render event processing, scene updates,
/// layout execution, and frame submission.
/// </summary>
internal sealed class RenderingPipeline : IRenderingPipeline
{
    private readonly RenderingOptions _options = new();

    private readonly ISceneGraph _scene;
    private readonly IAnimationSystem _anim;
    private readonly Camera.Camera _camera;
    private readonly ICameraController _cameraCtrl;
    private readonly ILayoutEngine _layout;
    private readonly ITranslationPipeline _translation;
    private readonly SceneCommandDispatcher _dispatcher;
    private readonly IRenderStateAssembler _assembler;

    private readonly RenderEventBuffer _events;
    private readonly SceneFrameUpdater _frameUpdater;
    private readonly RenderDiagnosticsRecorder _diagnosticsRecorder;
    private readonly RenderFrameAssembler _frameAssembler;
    private readonly HoverPickingService _hover;

    private RenderEventKinds _renderEvents = RenderEventKinds.Commit;

    public ITimelinePlayer Player { get; }

    private volatile bool _pendingClear;

    /// <summary>
    /// Creates rendering pipeline dependencies and playback integration.
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
        Vec2 viewportSize,
        IDiagnosticsProvider diagnostics)
    {
        Player = player;

        _scene = scene;
        _anim = anim;
        _layout = layout;
        _camera = camera;
        _cameraCtrl = cameraCtrl;
        _translation = translation;
        _assembler = assembler;

        _dispatcher = new SceneCommandDispatcher(
            handlers);

        var actorDecay = new ActorDecaySystem(
            diagnostics);

        _events = new RenderEventBuffer(
            _renderEvents);

        _frameUpdater = new SceneFrameUpdater(
            _scene,
            _anim,
            _layout,
            _cameraCtrl,
            actorDecay,
            viewportSize);

        _diagnosticsRecorder = new RenderDiagnosticsRecorder(
            diagnostics);

        _frameAssembler = new RenderFrameAssembler(
            _assembler,
            output);

        _hover = new HoverPickingService(
            _scene,
            _layout,
            _camera,
            viewportSize);

        Player.OnEvent += OnEvent;
        Player.OnStateChanged += OnStateChanged;
    }

    /// <summary>
    /// Configures which event kinds are rendered.
    /// </summary>
    public void SetRenderEvents(
        RenderEventKinds kinds)
    {
        _renderEvents = kinds;
        _events.SetRenderEvents(
            kinds);
    }

    /// <summary>
    /// Starts timeline playback.
    /// </summary>
    public void Start() =>
        Player.Play();

    /// <summary>
    /// Stops playback and clears render state.
    /// </summary>
    public void Stop()
    {
        Player.Stop();
        ClearSceneState();
    }

    /// <summary>
    /// Queues incoming playback events for rendering.
    /// </summary>
    private void OnEvent(
        TraceEvent evt) =>
        _events.Add(
            evt);

    /// <summary>
    /// Advances render the state for the current playback progress.
    /// </summary>
    internal void OnProgress(
        double progress)
    {
        if (_pendingClear)
        {
            _pendingClear = false;

            ClearSceneState();
        }

        _events.FlushTo(
            this);

        UpdateFrame();
    }

    /// <summary>
    /// Updates simulation, hover state, diagnostics, and frame output.
    /// </summary>
    private void UpdateFrame()
    {
        var diagnostics =
            Player.GetDiagnostics();

        float dt =
            _frameUpdater.Tick(
                diagnostics);

        _hover.Tick();

        _diagnosticsRecorder.Record(
            _scene,
            _anim,
            _layout,
            _camera,
            dt);

        _frameAssembler.SubmitFrame(
            diagnostics.PositionSeconds,
            dt,
            _scene,
            _anim,
            _camera,
            _cameraCtrl,
            diagnostics,
            _hover.HoveredNode,
            _hover.HoveredPod,
            _options.Mode);
    }

    /// <summary>
    /// Clears transient scene, animation, and hover state.
    /// </summary>
    private void ClearSceneState()
    {
        _scene.Clear();
        _anim.Clear();
        _assembler.Reset();
        _frameUpdater.ResetTime();
        _hover.Clear();
    }

    /// <summary>
    /// Updates hover picking mouse position.
    /// </summary>
    public void UpdateMouse(
        Vec2 screenPos)
    {
        _hover.UpdateMouse(
            screenPos);
    }

    /// <summary>
    /// Dispatches aggregated semantic events through translation pipelines.
    /// </summary>
    internal void DispatchAggregated<TEvent>(
        SemanticEventWriter<TEvent> writer)
        where TEvent : struct
    {
        var snapshot =
            writer.Snapshot();

        var span =
            snapshot.Span;

        foreach (var t in span)
        {
            ref readonly var evt = ref t;

            var timestamp =
                GetTimestamp(
                    evt);

            foreach (var command in _translation.Translate(evt))
            {
                _dispatcher.Dispatch(
                    command,
                    timestamp);
            }
        }
    }

    /// <summary>
    /// Resolves playback timestamp from semantic event type.
    /// </summary>
    private static double GetTimestamp<TEvent>(
        TEvent evt) =>
        evt switch
        {
            CommitBundleEvent c => c.Timestamp,
            BranchEvent b => b.Timestamp,
            MergeEvent m => m.Timestamp,
            FileCouplingEvent f => f.Timestamp,
            _ => throw new InvalidOperationException(
                $"Unknown event type {typeof(TEvent)}")
        };

    /// <summary>
    /// Handles playback state transitions.
    /// </summary>
    private void OnStateChanged(
        PlayerState state)
    {
    }

    /// <summary>
    /// Applies camera panning in world space.
    /// </summary>
    public void PanCamera(
        Vec2 delta)
    {
        _cameraCtrl.Pan(
            delta);
    }

    /// <summary>
    /// Applies relative camera zoom.
    /// </summary>
    public void ZoomCamera(
        float deltaZoom)
    {
        _cameraCtrl.Zoom(
            deltaZoom);
    }

    /// <summary>
    /// Sets absolute camera zoom and switches to free mode.
    /// </summary>
    public void SetZoom(
        float zoom)
    {
        _cameraCtrl.Mode =
            CameraFollowMode.Free;

        _camera.Zoom =
            Math.Clamp(
                zoom,
                0.15f,
                10.0f);
    }

    /// <summary>
    /// Changes active camera follow mode.
    /// </summary>
    public void SetCameraMode(
        CameraFollowMode mode)
    {
        _cameraCtrl.Mode =
            mode;
    }

    /// <summary>
    /// Toggles between supported layout visualization modes.
    /// </summary>
    public void ToggleLayoutMode()
    {
        _options.Mode =
            _options.Mode == LayoutMode.SingleTree
                ? LayoutMode.Forest
                : LayoutMode.SingleTree;
    }

    /// <summary>
    /// Returns the currently active layout mode.
    /// </summary>
    public LayoutMode GetLayoutMode() =>
        _options.Mode;

    /// <summary>
    /// Releases playback subscriptions and transient resources.
    /// </summary>
    public void Dispose()
    {
        Player.OnEvent -= OnEvent;
        Player.OnStateChanged -= OnStateChanged;

        _events.Dispose();
    }
}