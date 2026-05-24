using ChangeTrace.Graphics.Gpu.Pipelines;
using ChangeTrace.Graphics.Rendering.Passes;
using ChangeTrace.Graphics.Rendering.Renderers.Hud;
using ChangeTrace.Graphics.Rendering.Renderers.Sprites;
using ChangeTrace.Graphics.Rendering.Renderers.Text;
using ChangeTrace.Graphics.Rendering.Resources;
using ChangeTrace.Graphics.Rendering.Runtime;
using ChangeTrace.Graphics.Rendering.Scene;
using ChangeTrace.Graphics.Rendering.Text;
using ChangeTrace.Graphics.Shaders.Registry;
using ChangeTrace.Rendering;
using ChangeTrace.Rendering.Snapshots;
using ChangeTrace.Rendering.States;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace ChangeTrace.Graphics.Rendering;

/// <summary>
/// Main OpenTK rendering runtime.
/// Owns GPU pipelines, render passes, render resources
/// and orchestrates the full frame render graph.
/// </summary>
internal sealed class OpenTkRenderRuntime : IDisposable
{
    /// <summary>
    /// Maximum number of objects contributing to the heatmap.
    /// </summary>
    private const int MaxHeatmapObjects = 512;

    private RenderResources? _resources;

    private CircleGpuPipeline? _circlePipeline;
    private EdgeGpuPipeline? _edgePipeline;
    private ParticleGpuPipeline? _particlePipeline;
    private HeatmapGpuPipeline? _heatmapPipeline;
    private TextGpuPipeline? _textPipeline;

    private IconSpriteRenderer? _iconRenderer;
    private PodHoverCardRenderer? _podHoverCardRenderer;

    private readonly LodPlanner _lodPlanner = new();
    private readonly SceneVisibilitySystem _visibility = new();
    private readonly HeatmapCollector _heatmapCollector = new();
    private readonly RenderFrameGraph _frameGraph = new();

    private EdgeRenderPass? _edgePass;
    private NodeRenderPass? _nodePass;
    private HudRenderPass? _hudPass;

    private int _viewportWidth;
    private int _viewportHeight;

    private float _totalTime;

    /// <summary>
    /// Controls label rendering visibility.
    /// </summary>
    public bool ShowLabels { get; set; } = true;

    /// <summary>
    /// Controls HUD rendering visibility.
    /// </summary>
    public bool ShowHud { get; set; } = true;

    /// <summary>
    /// Current active layout mode.
    /// </summary>
    public LayoutMode Mode { get; private set; } = LayoutMode.SingleTree;

    /// <summary>
    /// Initializes rendering resources and GPU pipelines.
    /// </summary>
    public void Initialize(int viewportWidth, int viewportHeight)
    {
        _viewportWidth = viewportWidth;
        _viewportHeight = viewportHeight;

        _resources = new RenderResources(
                Path.Combine(
                    AppContext.BaseDirectory,
                    "atlas.png"),
                viewportWidth,
                viewportHeight);

        InitializePipelines();
        InitializePasses();
        InitializeOpenGlState();
    }

    /// <summary>
    /// Renders a complete frame.
    /// </summary>
    public void Render(RenderState state)
    {
        if (_resources == null ||
            _nodePass == null ||
            _edgePass == null ||
            _hudPass == null)
        {
            return;
        }

        _totalTime += (float)state.WallDelta;
        Mode = state.Mode;

        BeginFrame();

        // Main world space projection matrix.
        Matrix3 viewProjection = ViewProjection.Build(
                state.Camera.Position,
                state.Camera.Zoom,
                state.Camera.Rotation,
                _viewportWidth,
                _viewportHeight);

        // Screen space projection matrix.
        Matrix3 screenProjection = BuildScreenMatrix();

        // CPU side visible node selection.
        IReadOnlyList<NodeSnapshot> visibleNodes = _lodPlanner.Build(state, state.Camera.Zoom);

        _nodePass.SetVisibleNodes(visibleNodes);
        _edgePass.SetVisibleNodes(visibleNodes);

        _hudPass.ShowHud = ShowHud;
        _hudPass.ShowLabels = ShowLabels;

        var context = new RenderFrameContext(
                state,
                _resources,
                viewProjection,
                screenProjection,
                _viewportWidth,
                _viewportHeight,
                _totalTime);

        // Execute ordered render passes.
        _frameGraph.Execute(
            context);
    }

    /// <summary>
    /// Resizes viewport-dependent GPU resources.
    /// </summary>
    public void Resize(int width, int height)
    {
        _viewportWidth = width;
        _viewportHeight = height;

        GL.Viewport(
            0,
            0,
            width,
            height);

        _resources?.ResizeGpuTargets(
            width,
            height);

        RecreateHeatmapPipeline();
        RebuildPasses();
    }

    /// <summary>
    /// Creates all GPU pipelines.
    /// </summary>
    private void InitializePipelines()
    {
        _circlePipeline = new CircleGpuPipeline();
        _edgePipeline = new EdgeGpuPipeline();
        _particlePipeline = new ParticleGpuPipeline();
        _textPipeline = new TextGpuPipeline();

        RecreateHeatmapPipeline();
    }

    /// <summary>
    /// Initializes render passes.
    /// </summary>
    private void InitializePasses() =>
        RebuildPasses();

    /// <summary>
    /// Rebuilds the render graph and all render passes.
    /// </summary>
    private void RebuildPasses()
    {
        if (_resources == null ||
            _circlePipeline == null ||
            _edgePipeline == null ||
            _particlePipeline == null ||
            _heatmapPipeline == null ||
            _textPipeline == null)
        {
            return;
        }

        var hudRenderer = new HudRenderer(
                _textPipeline,
                _resources.Renderers.Text,
                _resources.Shaders.Compute("TextCull"),
                _resources.Shaders.Graphics("Text"));

        var labelRenderer = new LabelRenderer(
                _textPipeline,
                _resources.Renderers.Text,
                _resources.Shaders.Compute("TextCull"),
                _resources.Shaders.Graphics("Text"));

        _iconRenderer ??= LanguageIconAtlasLoader.LoadOrNull();

        _podHoverCardRenderer ??=
            new PodHoverCardRenderer(
                _textPipeline,
                _iconRenderer);

        var backgroundPass = new BackgroundRenderPass();

        var heatmapPass = new HeatmapRenderPass(
            _heatmapPipeline,
            _heatmapCollector,
            MaxHeatmapObjects);

        _edgePass = new EdgeRenderPass(_edgePipeline, _visibility);
        _nodePass = new NodeRenderPass(_circlePipeline, _visibility);

        var particlePass = new ParticleRenderPass(_particlePipeline);

        _hudPass = new HudRenderPass(
            hudRenderer,
            labelRenderer,
            _podHoverCardRenderer);

        _frameGraph.Clear();

        // Ordered frame rendering pipeline.
        _frameGraph.Add(backgroundPass);
        _frameGraph.Add(heatmapPass);
        _frameGraph.Add(_edgePass);
        _frameGraph.Add(_nodePass);
        _frameGraph.Add(particlePass);
        _frameGraph.Add(_hudPass);
    }

    /// <summary>
    /// Recreates the low resolution heatmap pipeline.
    /// </summary>
    private void RecreateHeatmapPipeline()
    {
        if (_resources == null)
            return;

        _heatmapPipeline?.Dispose();

        _heatmapPipeline = new HeatmapGpuPipeline(
                Math.Max(1, _viewportWidth / 4),
                Math.Max(1, _viewportHeight / 4),
                MaxHeatmapObjects,
                _resources.Shaders.Compute("Heatmap"));
    }

    /// <summary>
    /// Configures persistent OpenGL render state.
    /// </summary>
    private static void InitializeOpenGlState()
    {
        GL.Enable(EnableCap.Blend);

        GL.BlendFunc(
            BlendingFactor.SrcAlpha,
            BlendingFactor.OneMinusSrcAlpha);

        GL.ClearColor(0.04f, 0.04f, 0.06f, 1.0f);
    }

    /// <summary>
    /// Begins new frame render.
    /// </summary>
    private static void BeginFrame()
    {
        GL.Disable(EnableCap.DepthTest);

        GL.Clear(ClearBufferMask.ColorBufferBit);
    }

    /// <summary>
    /// Builds the screen space projection matrix.
    /// </summary>
    private Matrix3 BuildScreenMatrix()
    {
        var matrix = Matrix3.Identity;

        matrix.M11 = 2.0f / _viewportWidth;
        matrix.M22 = -2.0f / _viewportHeight;
        matrix.M31 = -1.0f;
        matrix.M32 = 1.0f;

        return matrix;
    }

    /// <summary>
    /// Releases GPU resources and render objects.
    /// </summary>
    public void Dispose()
    {
        _circlePipeline?.Dispose();
        _edgePipeline?.Dispose();
        _particlePipeline?.Dispose();
        _heatmapPipeline?.Dispose();
        _textPipeline?.Dispose();

        _podHoverCardRenderer?.Dispose();
        _iconRenderer?.Dispose();

        _resources?.Dispose();
    }
}