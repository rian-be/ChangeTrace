using ChangeTrace.Core.Diagnostics;
using ChangeTrace.Core.Timelines;
using ChangeTrace.Graphics.Input;
using ChangeTrace.Player;
using ChangeTrace.Player.Enums;
using ChangeTrace.Player.Factory;
using ChangeTrace.Rendering;
using ChangeTrace.Rendering.Factory;
using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Pipeline;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace ChangeTrace.Graphics.Window;

/// <summary>
/// Main OpenTK visualization window.
/// Owns window lifecycle and forwards update/render/input work to runtime systems.
/// </summary>
internal sealed class PlayerWindow : GameWindow
{
    private readonly Timeline _timeline;

    private readonly ITimelinePlayerFactory _playerFactory;
    private readonly IRenderSystemFactory _renderFactory;
    private readonly IDiagnosticsProvider _diagnostics;

    private readonly PlayerInputController _input = new();

    private TimelinePlayer? _player;
    private RenderingPipeline? _pipeline;
    private IRenderOutput? _renderer;

    private DebugWindow? _debugWindow;

    private bool _initialized;

    internal PlayerWindow(
        Timeline timeline,
        ITimelinePlayerFactory playerFactory,
        IRenderSystemFactory renderFactory,
        IDiagnosticsProvider diagnostics)
        : base(
            CreateGameSettings(),
            CreateNativeSettings())
    {
        _timeline = timeline;
        _playerFactory = playerFactory;
        _renderFactory = renderFactory;
        _diagnostics = diagnostics;
    }

    /// <summary>
    /// Initializes rendering systems and playback runtime.
    /// </summary>
    protected override void OnLoad()
    {
        base.OnLoad();

        InitializeRendering();
        InitializePlayer();
        InitializePipeline();

        _initialized = true;
    }

    /// <summary>
    /// Releases runtime resources and GPU objects.
    /// </summary>
    protected override void OnUnload()
    {
        _pipeline?.Dispose();
        _player?.Dispose();

        if (_renderer is IDisposable disposable)
        {
            disposable.Dispose();
        }

        base.OnUnload();
    }

    /// <summary>
    /// Updates input and runtime systems.
    /// </summary>
    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);

        if (_player == null ||
            _pipeline == null)
        {
            return;
        }

        _input.HandleKeyboard(KeyboardState, _player, _pipeline, Close);
        _debugWindow?.ManualUpdate();
    }

    /// <summary>
    /// Renders the current frame.
    /// </summary>
    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);

        if (_pipeline != null)
        {
            var diagnostics = _pipeline.Player.GetDiagnostics();

            _pipeline.OnProgress(diagnostics.Progress);
        }

        RecordFrameMetrics(e.Time);

        SwapBuffers();

        // Render debug overlay/context separately.
        _debugWindow?.ManualRender();
        Context.MakeCurrent();
    }

    /// <summary>
    /// Handles viewport resize events.
    /// </summary>
    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        _renderer?.Resize(e.Width, e.Height);
    }

    /// <summary>
    /// Handles mouse wheel zoom input.
    /// </summary>
    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        if (_pipeline == null)
            return;

        _input.HandleMouseWheel(e, _pipeline);
    }

    /// <summary>
    /// Handles mouse button press events.
    /// </summary>
    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);

        if (_pipeline == null)
            return;

        _input.HandleMouseDown(e, MousePosition, ClientSize, _pipeline);
    }

    /// <summary>
    /// Handles mouse button release events.
    /// </summary>
    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);

        _input.HandleMouseUp(e);
    }

    /// <summary>
    /// Handles mouse movement updates.
    /// </summary>
    protected override void OnMouseMove(MouseMoveEventArgs e)
    {
        base.OnMouseMove(e);

        if (_pipeline == null)
            return;

        _input.HandleMouseMove(MousePosition, ClientSize, _pipeline);
    }

    /// <summary>
    /// Attaches a manual debug window renderer.
    /// </summary>
    internal void SetDebugWindow(DebugWindow debugWindow)
    {
        _debugWindow = debugWindow;

        if (!_initialized)
            return;

        _debugWindow.InitializeManual();

        Context.MakeCurrent();
    }

    /// <summary>
    /// Initializes renderer and OpenGL state.
    /// </summary>
    private void InitializeRendering()
    {
        VSync = VSyncMode.Off;
        Context.SwapInterval = 0;

        _debugWindow?.InitializeManual();

        Context.MakeCurrent();

        var (_, _, _, _, _, _, _, _, renderer) = _renderFactory.Create();

        _renderer = renderer;

        _renderer.Initialize(Size.X, Size.Y);
        _renderer.Resize(Size.X, Size.Y);
    }

    /// <summary>
    /// Creates the timeline playback controller.
    /// </summary>
    private void InitializePlayer() =>
        _player = (TimelinePlayer)_playerFactory.Create(
                _timeline,
                mode: PlaybackMode.Once,
                initialSpeed: 1.0,
                acceleration: 4.0,
                secondsPerDay: 45);

    /// <summary>
    /// Creates and starts the rendering pipeline.
    /// </summary>
    private void InitializePipeline()
    {
        if (_player == null || _renderer == null)
        {
            return;
        }

        var (
            scene,
            anim,
            camera,
            cameraCtrl,
            assembler,
            handlers,
            layout,
            translation,
            _) = _renderFactory.Create();

        _pipeline = new RenderingPipeline(
                _player,
                _renderer,
                layout,
                camera,
                cameraCtrl,
                scene,
                anim,
                translation,
                assembler,
                handlers,
                new Vec2(
                    Size.X,
                    Size.Y),
                _diagnostics);

        _pipeline.Start();

        _debugWindow?.SetClearAction(() => scene.ClearAvatars());
    }

    /// <summary>
    /// Records runtime frame diagnostics.
    /// </summary>
    private void RecordFrameMetrics(double frameTime)
    {
        _diagnostics.RecordMetric("Perf.FPS", 1.0 / frameTime);
        _diagnostics.RecordMetric("Perf.FrameTimeMs", frameTime * 1000.0);
    }

    /// <summary>
    /// Creates OpenTK game loop settings.
    /// </summary>
    private static GameWindowSettings CreateGameSettings()
    {
        return new GameWindowSettings
        {
            UpdateFrequency = 144.0
        };
    }

    /// <summary>
    /// Creates a native window and OpenGL context settings.
    /// </summary>
    private static NativeWindowSettings CreateNativeSettings()
    {
        return new NativeWindowSettings
        {
            Title = "ChangeTrace",

            ClientSize = new Vector2i(
                1280,
                720),

            Profile = ContextProfile.Core,

            APIVersion = new Version(
                3,
                3),

            Flags = ContextFlags.ForwardCompatible,

            NumberOfSamples = 4,

            Vsync = VSyncMode.Off
        };
    }
}