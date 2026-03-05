using ChangeTrace.Core;
using ChangeTrace.Player;
using ChangeTrace.Player.Enums;
using ChangeTrace.Player.Interfaces;
using ChangeTrace.Rendering;
using ChangeTrace.Rendering.Camera;
using ChangeTrace.Rendering.Layout;
using ChangeTrace.OpenTK.Renderer;
using ChangeTrace.Player.Factory;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Factory;
using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Outputs;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace ChangeTrace.OpenTK.Window;

/// <summary>
/// Top-level OpenTK window.
/// Wires: GameWindow → RenderingPipeline → IRenderOutput (OpenTkRenderOutput).
///
/// SRP: owns window lifecycle, input handling, and GL swap only.
///      No rendering logic lives here — delegated to <see zcref="OpenTkRenderOutput"/>.
/// </summary>
public sealed class PlayerWindow : GameWindow
{
    private readonly ITimelinePlayerFactory _playerFactory;
    private readonly IRenderSystemFactory _renderFactory;

    private readonly Timeline _timeline;
    private TimelinePlayer? _player;
    private RenderingPipeline? _pipeline;
    private IRenderOutput? _renderer; 

    private Vector2 _lastMouse;
    private bool _dragging;

    [Obsolete("Obsolete")]
    internal PlayerWindow(
        Timeline timeline,
        ITimelinePlayerFactory playerFactory,
        IRenderSystemFactory renderFactory)
        : base(
            new GameWindowSettings { UpdateFrequency = 60 },
            new NativeWindowSettings
            {
                Title = "ChangeTrace",
                Size = new Vector2i(1280, 720),
                Profile = ContextProfile.Core,
                APIVersion = new Version(3, 3),
                Flags = ContextFlags.ForwardCompatible
            })
    {
        _timeline = timeline;
        _playerFactory = playerFactory;
        _renderFactory = renderFactory;
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        _player = (TimelinePlayer)_playerFactory.Create(
            _timeline,
            mode: PlaybackMode.Once,
            initialSpeed: 0.10,
            acceleration: 2.0,
            secondsPerDay: 30);

        var (scene, anim, camera, cameraCtrl, assembler, handlers, layout, translation, renderer) =
            _renderFactory.Create();

        _renderer = renderer;
        _renderer.Initialize(ClientSize.X, ClientSize.Y);
        
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
            new Vec2(ClientSize.X, ClientSize.Y));

        _pipeline.Start();
    }

    protected override void OnUnload()
    {
        _pipeline?.Dispose();
        _player?.Dispose();
        if (_renderer is IDisposable disposableRenderer)
            disposableRenderer.Dispose();
        base.OnUnload();
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);
        HandleKeyboard((float)e.Time);
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);

        if (_pipeline != null)
        {
            var diag = _pipeline.Player.GetDiagnostics();
            _pipeline.OnProgress(diag.Progress);
        }

        SwapBuffers();
    }
    
    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        _renderer.Resize(e.Width, e.Height);
    }

    // ------------------------------------------------------------------
    // Input
    // ------------------------------------------------------------------
private void HandleKeyboard(float dt)
{
    if (_player == null) return;
    var kb = KeyboardState;

    // Transport
    if (kb.IsKeyPressed(Keys.Space))
    {
        if (_player.State == PlayerState.Playing)
        {
            _player.Pause();
            Console.WriteLine("[Keyboard] Space pressed → Pause");
        }
        else
        {
            _player.Play();
            Console.WriteLine("[Keyboard] Space pressed → Play");
        }
    }

    if (kb.IsKeyPressed(Keys.R))
    {
        if (kb.IsKeyDown(Keys.LeftShift))
        {
            _player.Play();
            Console.WriteLine("[Keyboard] Shift + R pressed → Play");
        }
        else
        {
            _player.Stop();
            Console.WriteLine("[Keyboard] R pressed → Stop");
        }
    }

    if (kb.IsKeyPressed(Keys.Equal) || kb.IsKeyPressed(Keys.KeyPadAdd))
    {
        _player.TargetSpeed = Math.Min(_player.TargetSpeed * 2.0, 100.0);
        Console.WriteLine($"[Keyboard] + pressed → TargetSpeed = {_player.TargetSpeed}");
    }

    if (kb.IsKeyPressed(Keys.Minus) || kb.IsKeyPressed(Keys.KeyPadSubtract))
    {
        _player.TargetSpeed = Math.Max(_player.TargetSpeed / 2.0, 0.1);
        Console.WriteLine($"[Keyboard] - pressed → TargetSpeed = {_player.TargetSpeed}");
    }

    // Presets
    if (kb.IsKeyPressed(Keys.D1))
    {
        _player.ApplyPreset(SpeedPreset.Normal);
        Console.WriteLine("[Keyboard] D1 pressed → Preset: Normal");
    }
    if (kb.IsKeyPressed(Keys.D2))
    {
        _player.ApplyPreset(SpeedPreset.Double);
        Console.WriteLine("[Keyboard] D2 pressed → Preset: Double");
    }
    if (kb.IsKeyPressed(Keys.D5))
    {
        _player.ApplyPreset(SpeedPreset.Fast);
        Console.WriteLine("[Keyboard] D5 pressed → Preset: Fast");
    }
    if (kb.IsKeyPressed(Keys.D0))
    {
        _player.ApplyPreset(SpeedPreset.Scrub);
        Console.WriteLine("[Keyboard] D0 pressed → Preset: Scrub");
    }
    
    
    if (kb.IsKeyPressed(Keys.F1))
    {
        _pipeline?.SetCameraMode(CameraFollowMode.FollowAverage);
        Console.WriteLine("[Camera] Mode → FollowAverage");
    }
    if (kb.IsKeyPressed(Keys.F2))
    {
        _pipeline?.SetCameraMode(CameraFollowMode.FollowActive);
        Console.WriteLine("[Camera] Mode → FollowActive");
    }
    if (kb.IsKeyPressed(Keys.F3))
    {
        _pipeline?.SetCameraMode(CameraFollowMode.FitAll);
        Console.WriteLine("[Camera] Mode → FitAll");
    }
    if (kb.IsKeyPressed(Keys.F4))
    {
        _pipeline?.SetCameraMode(CameraFollowMode.Free);
        Console.WriteLine("[Camera] Mode → Manual");
    }
    

    // Step (only when paused)
    if (kb.IsKeyPressed(Keys.Right))
    {
        _player.StepForward();
        Console.WriteLine("[Keyboard] Right arrow pressed → Step Forward");
    }
    if (kb.IsKeyPressed(Keys.Left))
    {
        _player.StepBackward();
        Console.WriteLine("[Keyboard] Left arrow pressed → Step Backward");
    }

    // Seek ±10% of timeline
    if (kb.IsKeyPressed(Keys.Period))
    {
        _player.SeekRelative(_player.DurationSeconds * 0.1);
        Console.WriteLine("[Keyboard] Period (.) pressed → Seek +10%");
    }
    if (kb.IsKeyPressed(Keys.Comma))
    {
        _player.SeekRelative(-_player.DurationSeconds * 0.1);
        Console.WriteLine("[Keyboard] Comma (,) pressed → Seek -10%");
    }

    // Escape
    if (kb.IsKeyPressed(Keys.Escape))
    {
        Console.WriteLine("[Keyboard] Escape pressed → Closing window");
        Close();
    }
}

    private void HandleKeyboard2(float dt)
    {
        if (_player == null) return;
        var kb = KeyboardState;

        // Transport
        if (kb.IsKeyPressed(Keys.Space))
        {
            if (_player.State == PlayerState.Playing) _player.Pause();
            else                                       _player.Play();
        }
        if (kb.IsKeyPressed(Keys.R))   _player.Stop();
        if (kb.IsKeyPressed(Keys.R) && kb.IsKeyDown(Keys.LeftShift)) _player.Play();

        if (kb.IsKeyPressed(Keys.Equal) || kb.IsKeyPressed(Keys.KeyPadAdd))
            _player.TargetSpeed = Math.Min(_player.TargetSpeed * 2.0, 100.0);

        if (kb.IsKeyPressed(Keys.Minus) || kb.IsKeyPressed(Keys.KeyPadSubtract))
            _player.TargetSpeed = Math.Max(_player.TargetSpeed / 2.0, 0.1);

        // Presets
        if (kb.IsKeyPressed(Keys.D1)) _player.ApplyPreset(SpeedPreset.Normal);
        if (kb.IsKeyPressed(Keys.D2)) _player.ApplyPreset(SpeedPreset.Double);
        if (kb.IsKeyPressed(Keys.D5)) _player.ApplyPreset(SpeedPreset.Fast);
        if (kb.IsKeyPressed(Keys.D0)) _player.ApplyPreset(SpeedPreset.Scrub);

        // Step (only when paused)
        if (kb.IsKeyPressed(Keys.Right)) _player.StepForward();
        if (kb.IsKeyPressed(Keys.Left))  _player.StepBackward();

        // Seek ±10% of timeline
        if (kb.IsKeyPressed(Keys.Period)) _player.SeekRelative( _player.DurationSeconds * 0.1);
        if (kb.IsKeyPressed(Keys.Comma))  _player.SeekRelative(-_player.DurationSeconds * 0.1);

        // Escape
        if (kb.IsKeyPressed(Keys.Escape)) Close();
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);
        
        _pipeline?.ZoomCamera(e.OffsetY * 0.1f);
        // Camera zoom forwarded to pipeline (TODO: expose camera controls through pipeline)
        // _pipeline?.ZoomAt(e.OffsetY * 0.1f, mouseWorldPos);
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButton.Left) { _dragging = true; _lastMouse = MousePosition; }
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button == MouseButton.Left) _dragging = false;
    }

    protected override void OnMouseMove(MouseMoveEventArgs e)
    {
        base.OnMouseMove(e);
        if (!_dragging || _pipeline == null)
            return;

        var current = MousePosition;
        var deltaScreen = current - _lastMouse;
        _lastMouse = current;

        // konwersja screen → world
        var worldDelta = new Vec2(-deltaScreen.X, deltaScreen.Y) * 1f; 
        _pipeline.PanCamera(worldDelta);
    }
}