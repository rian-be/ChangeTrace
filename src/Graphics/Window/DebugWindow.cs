using ChangeTrace.Core.Diagnostics;
using ChangeTrace.Graphics.Gpu.Pipelines;
using ChangeTrace.Graphics.Rendering.Renderers.Text;
using ChangeTrace.Graphics.Shaders.Registry;
using ChangeTrace.Graphics.Shaders.Runtime;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using MouseButton = OpenTK.Windowing.GraphicsLibraryFramework.MouseButton;

namespace ChangeTrace.Graphics.Window;

/// <summary>
/// Debug visualization window.
/// </summary>
internal sealed class DebugWindow(
    IDiagnosticsProvider diagnostics,
    Action? onClearAvatars = null)
    : GameWindow(
        new GameWindowSettings
        {
            UpdateFrequency = 0
        },
        new NativeWindowSettings
        {
            Title = "ChangeTrace - DEBUG ENGINE (SYSTEM ACTIVE)",
            ClientSize = new Vector2i(400, 900),
            Profile = ContextProfile.Core,
            APIVersion = new Version(3, 3),
            Vsync = VSyncMode.Off
        })
{
    private const int MaxHistory = 50;

    private enum DebugTab
    {
        Engine,
        Perf,
        Scene,
        Hotspots
    }

    private readonly Queue<double> _fpsHistory = [];
    private readonly Queue<double> _memHistory = [];
    private readonly Queue<double> _cpuHistory = [];
    private readonly Queue<double> _frameHistory = [];

    private TextGpuPipeline? _textGpu;
    private TextRenderer? _text;
    private ComputeShader? _textComputeShader;
    private ShaderProgram? _textShader;
    private ShaderResources? _shaders;

    private DebugTab _currentTab =
        DebugTab.Engine;

    private bool _showFullPaths;

    private Action? _onClearAvatars =
        onClearAvatars;

    /// <summary>
    /// Sets avatar purge callback.
    /// </summary>
    public void SetClearAction(Action action) => 
        _onClearAvatars = action;
    

    /// <summary>
    /// Initializes debug rendering resources.
    /// </summary>
    protected override void OnLoad()
    {
        base.OnLoad();

        VSync = VSyncMode.Off;
        Context.SwapInterval = 0;

        InitializeManual();
    }

    /// <summary>
    /// Initializes GPU resources manually.
    /// </summary>
    public void InitializeManual()
    {
        MakeCurrent();
        GL.ClearColor(0.05f, 0.05f, 0.07f, 1.0f);

        _shaders = new ShaderResources();
        _textGpu = new TextGpuPipeline();
        _text = new TextRenderer(Path.Combine( AppContext.BaseDirectory, "atlas.png"));

        _textComputeShader = _shaders.Compute("TextCull");
        _textShader = _shaders.Graphics("Text");
    }

    /// <summary>
    /// Updates debug window events manually.
    /// </summary>
    public void ManualUpdate()
    {
        if (IsExiting)
            return;

        ProcessEvents(0);
    }

    /// <summary>
    /// Renders debug UI.
    /// </summary>
    public void ManualRender()
    {
        if (IsExiting || _textGpu == null || _text == null ||
            _textComputeShader == null ||
            _textShader == null)
        {
            return;
        }

        MakeCurrent();

        GL.Clear(ClearBufferMask.ColorBufferBit);

        var mem = diagnostics.GetMemoryMetrics();
        var run = diagnostics.GetRuntimeMetrics();
        var gc = diagnostics.GetGcCollections();

        var screenMatrix = new Matrix3(2f / ClientSize.X, 0f, -1f, 0f, -2f / ClientSize.Y, 1f, 0f, 0f, 1f);

        float x = 20;
        float y = 20;

        var white = new Vector4(0.9f, 0.9f, 0.9f, 1f);
        var accent = new Vector4(0.2f, 0.7f, 1.0f, 1f);
        var green = new Vector4(0.4f, 1.0f, 0.4f, 1f);

        _textGpu.Begin();

        DrawTabHeader("ENGINE", DebugTab.Engine, ref x, ref y);
        DrawTabHeader("PERF", DebugTab.Perf, ref x, ref y);
        DrawTabHeader("SCENE", DebugTab.Scene, ref x, ref y);
        DrawTabHeader("HOT", DebugTab.Hotspots, ref x, ref y);

        x = 20;
        y = 70;

        var custom = diagnostics.GetCustomMetrics();

        switch (_currentTab)
        {
            case DebugTab.Engine:
            {
                DrawHeader("RUNTIME ENGINE", ref x, ref y, accent);

                DrawStat("UpTime", $"{TimeSpan.FromSeconds(run.UpTimeSeconds):hh\\:mm\\:ss}", ref x, ref y, white);

                DrawStat("Decay Delta", $"{custom.GetValueOrDefault("Decay.Delta", 0):F5}", ref x, ref y, white);
                DrawStat("Decay Tick", $"{custom.GetValueOrDefault("Decay.TickCount", 0)}", ref x, ref y, white);

                UpdateHistory(_cpuHistory, run.CpuUsagePercentage);

                DrawStat("CPU Usage", $"{run.CpuUsagePercentage:F1}%", ref x, ref y, run.CpuUsagePercentage > 80
                        ? new Vector4(1, 0, 0, 1)
                        : green);

                DrawSparkline(_cpuHistory, 0, 100, ref x, ref y, run.CpuUsagePercentage > 80
                        ? new Vector4(1, 0, 0, 1)
                        : green);

                DrawStat("Threads", run.ThreadCount.ToString(), ref x, ref y, white);
                DrawStat("Handles", run.HandleCount.ToString(), ref x, ref y, white);

                double wallDelta = custom.GetValueOrDefault("Scene.WallDelta", 0);

                DrawStat("Wall Delta", $"{wallDelta:F4}s", ref x, ref y, wallDelta <= 0
                        ? new Vector4(1, 0, 0, 1)
                        : white);

                y += 15;

                DrawHeader("MEMORY MANAGEMENT", ref x, ref y, accent);
                DrawStat("Managed", $"{mem.ManagedMb:F1} MB", ref x, ref y, white);
                DrawStat("WorkingSet", $"{mem.WorkingSetMb:F1} MB", ref x, ref y, white);
                DrawStat("Private", $"{mem.PrivateBytesMb:F1} MB", ref x, ref y, white);

                y += 15;

                DrawHeader("GARBAGE COLLECTOR", ref x, ref y, accent);
                DrawStat("Gen 0", gc[0].ToString(), ref x, ref y, white);
                DrawStat("Gen 1", gc[1].ToString(), ref x, ref y, white);
                DrawStat("Gen 2", gc[2].ToString(), ref x, ref y, white);

                break;
            }

            case DebugTab.Perf:
            {
                DrawHeader("PERFORMANCE", ref x, ref y, accent);

                double fps = custom.GetValueOrDefault("Perf.FPS", 0);

                UpdateHistory(_fpsHistory, fps);

                DrawStat("FPS", $"{fps:F1}", ref x, ref y, fps < 30
                        ? new Vector4(1, 0.5f, 0, 1)
                        : green);

                DrawSparkline(_fpsHistory, 0, 144, ref x, ref y, green);

                double frameTime = custom.GetValueOrDefault("Perf.FrameTimeMs", 0);

                UpdateHistory(_frameHistory, frameTime);

                DrawStat("FrameTime", $"{frameTime:F2} ms", ref x, ref y, frameTime > 33
                        ? new Vector4(1, 0.5f, 0, 1)
                        : white);

                DrawSparkline(_frameHistory, 0, 50, ref x, ref y, white);

                y += 15;

                DrawHeader("MEMORY TREND (Managed)", ref x, ref y, accent);

                UpdateHistory(_memHistory, mem.ManagedMb);

                DrawStat("Managed",
                    $"{mem.ManagedMb:F1} MB",
                    ref x,
                    ref y,
                    white);

                DrawSparkline(_memHistory,
                    0,
                    512,
                    ref x,
                    ref y,
                    accent);

                break;
            }
        }

        int glyphCount = _textGpu.Upload();

        _textGpu.Execute(_textComputeShader,
            glyphCount,
            screenMatrix,
            new Vector2(
                ClientSize.X,
                ClientSize.Y));

        _text.DrawGpu(_textGpu, _textShader, screenMatrix);
        SwapBuffers();
    }

    /// <summary>
    /// Handles debug UI mouse interactions.
    /// </summary>
    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);

        if (e.Button != MouseButton.Left)
            return;

        var pos = MousePosition;

        if (pos.Y is >= 10 and <= 60)
        {
            _currentTab =
                pos.X switch
                {
                    >= 20 and <= 110 => DebugTab.Engine,
                    > 110 and <= 200 => DebugTab.Perf,
                    > 200 and <= 290 => DebugTab.Scene,
                    > 290 and <= 380 => DebugTab.Hotspots,
                    _ => _currentTab
                };
        }
    }

    /// <summary>
    /// Draws tab header button.
    /// </summary>
    private void DrawTabHeader(string text, DebugTab tab, ref float x, ref float y)
    {
        if (_textGpu == null)
            return;

        var color = _currentTab == tab
                ? new Vector4(1, 1, 0, 1)
                : new Vector4(0.5f, 0.5f, 0.5f, 1);

        _textGpu.DrawString(text,
            new Vector2(x, y),
            0.6f,
            color);

        x += 90;
    }

    /// <summary>
    /// Updates bounded metric history.
    /// </summary>
    private static void UpdateHistory(Queue<double> history, double value)
    {
        history.Enqueue(value);

        while (history.Count > MaxHistory)
        {
            history.Dequeue();
        }
    }

    /// <summary>
    /// Draws sparkline graph.
    /// </summary>
    private void DrawSparkline(Queue<double> history, double min, double max, ref float x, ref float y, Vector4 color)
    {
        if (_textGpu == null)
            return;

        if (history.Count < 2)
        {
            y += 30;
            return;
        }

        float width = 300;
        float height = 25;
        float step = width / MaxHistory;

        var values = history.ToArray();

        for (int i = 0; i < values.Length - 1; i++)
        {
            float v = (float)((values[i] - min) / (max - min));

            v = Math.Clamp(v, 0, 1);

            if (i % 5 == 0)
            {
                _textGpu.DrawString(".",
                    new Vector2(
                        x + i * step,
                        y + height - v * height),
                    0.5f,
                    color);
            }
        }

        y += height + 10;
    }

    /// <summary>
    /// Draws section header.
    /// </summary>
    private void DrawHeader(string text, ref float x, ref float y, Vector4 color)
    {
        if (_textGpu == null)
            return;

        _textGpu.DrawString(text, new Vector2(x, y), 0.9f, color);
        y += 25;
    }

    /// <summary>
    /// Draws metric row.
    /// </summary>
    private void DrawStat(string label, string value, ref float x, ref float y, Vector4 color)
    {
        if (_textGpu == null)
            return;

        _textGpu.DrawString($"{label}:",
            new Vector2(x, y),
            0.7f,
            new Vector4(0.6f, 0.6f, 0.6f, 1f));

        _textGpu.DrawString(value,
            new Vector2(x + 120, y),
            0.7f,
            color);

        y += 15;
    }

    /// <summary>
    /// Updates OpenGL viewport.
    /// </summary>
    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        GL.Viewport(0, 0, e.Width, e.Height);
    }

    /// <summary>
    /// Releases GPU resources.
    /// </summary>
    protected override void OnUnload()
    {
        base.OnUnload();

        _textGpu?.Dispose();
        _text?.Dispose();
        _shaders?.Dispose();

        _textGpu = null;
        _text = null;
        _textComputeShader = null;
        _textShader = null;
        _shaders = null;
    }
}