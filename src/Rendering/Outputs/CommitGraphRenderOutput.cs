using ChangeTrace.Configuration.Discovery;
using ChangeTrace.OpenTK.Renderer;
using ChangeTrace.OpenTK.Shaders;
using ChangeTrace.OpenTK.Text;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Snapshots;
using ChangeTrace.Rendering.States;
using Microsoft.Extensions.DependencyInjection;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SysVec4 = System.Numerics.Vector4;

namespace ChangeTrace.Rendering.Outputs;

/// <summary>
/// Renderer dedykowany dla CommitGraphLayout - radialny układ koncentryczny.
/// Kolory i stylizacja dopasowane do wizualizacji z centrum pomarańczowym,
/// środkiem cyan/purple i zewnętrznym pierścieniem niebieskim.
/// </summary>
//[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class CommitGraphRenderOutput : IRenderOutput, IDisposable
{
    // ------------------------------------------------------------------
    // Sub-renderers
    // ------------------------------------------------------------------
    private CircleRenderer   _circles;
    private EdgeRenderer     _edges;
    private ParticleRenderer _particles;
    private TextRenderer     _text;

    // ------------------------------------------------------------------
    // Shaders
    // ------------------------------------------------------------------
    private ShaderProgram _circleShader;
    private ShaderProgram _edgeShader;
    private ShaderProgram _particleShader;
    private ShaderProgram _textShader;

    // ------------------------------------------------------------------
    // Viewport
    // ------------------------------------------------------------------
    private int _viewW;
    private int _viewH;

    // Config
    internal bool  ShowLabels   { get; set; } = false;  // Domyślnie wyłączone dla czystości
    internal bool  ShowHud      { get; set; } = true;
    internal float LabelScale   { get; set; } = 0.25f;
    internal float LabelMinZoom { get; set; } = 0.3f;

    private string _fontAtlasPath = "atlas.png";

    public CommitGraphRenderOutput() { }

    public void Initialize(int viewportW, int viewportH)
    {
        _viewW = viewportW;
        _viewH = viewportH;

        _circleShader   = new ShaderProgram(ShaderSource.CircleVert,   ShaderSource.CircleFrag);
        _edgeShader     = new ShaderProgram(ShaderSource.EdgeVert,     ShaderSource.EdgeFrag);
        _particleShader = new ShaderProgram(ShaderSource.ParticleVert, ShaderSource.ParticleFrag);
        _textShader     = new ShaderProgram(ShaderSource.TextVert,     ShaderSource.TextFrag);

        _circles   = new CircleRenderer();
        _edges     = new EdgeRenderer();
        _particles = new ParticleRenderer();
        _text      = new TextRenderer(_fontAtlasPath);

        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.ClearColor(0.04f, 0.04f, 0.06f, 1f);
    }

    // ------------------------------------------------------------------
    // IRenderOutput
    // ------------------------------------------------------------------
    public void Submit(RenderState state)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit);

        var cam      = state.Camera;
        var scene    = state.Scene;
        var viewProj = ViewProjection.Build(cam.Position, cam.Zoom, cam.Rotation, _viewW, _viewH);

        // 1. Subtelne radialne krawędzie (very subtle)
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        var radialEdges = BuildRadialEdges(scene).ToList();
        _edges.Draw(radialEdges, scene, _edgeShader, viewProj);

        // 2. Dynamiczne krawędzie commitów (additive glow)
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
        _edges.Draw(scene.VisibleEdges().ToList(), scene, _edgeShader, viewProj);

        // 3. Węzły ze stylizacją radialną (kolory zależne od odległości)
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        var styledNodes = BuildRadialStyledNodes(scene.Nodes);
        _circles.Draw(styledNodes, _circleShader, viewProj);

        // 4. Bloom na świecących węzłach (additive)
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
        _circles.DrawBloom(styledNodes, _circleShader, viewProj);

        // 5. Avatary deweloperów
        var avatarNodes = BuildAvatarNodes(scene.VisibleAvatars().ToList());
        _circles.Draw(avatarNodes, _circleShader, viewProj);

        // 6. Cząsteczki
        _particles.Draw(scene.Particles, _particleShader, viewProj);

        // 7. Etykiety (opcjonalne)
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        if (ShowLabels && cam.Zoom >= LabelMinZoom)
            DrawLabels(scene, viewProj);

        // 8. HUD
        if (ShowHud)
            DrawHud(state, BuildScreenMatrix());
    }

    // ------------------------------------------------------------------
    // Radialne krawędzie - subtelne połączenia między warstwami
    // ------------------------------------------------------------------
    private static IEnumerable<EdgeSnapshot> BuildRadialEdges(ISceneSnapshot scene)
    {
        var allNodes = scene.Nodes.ToList();
        
        // Grupuj pliki według katalogów dla hub-and-spoke w obrębie sektorów
        var fileNodes = allNodes.Where(n => n.Kind == NodeKind.File).ToList();
        var filesByDir = fileNodes.GroupBy(f => GetParentPath(f.Id));
        
        var fileColor = new SysVec4(0.3f, 0.3f, 0.5f, 0.12f); // Bardzo subtle

        foreach (var group in filesByDir)
        {
            var files = group.ToList();
            if (files.Count < 2) continue;

            // Hub = plik najbliższy środkowi grupy
            var cx = files.Average(f => f.Position.X);
            var cy = files.Average(f => f.Position.Y);
            var hub = files.OrderBy(f =>
            {
                var dx = f.Position.X - cx;
                var dy = f.Position.Y - cy;
                return dx * dx + dy * dy;
            }).First();

            // Linie od huba do każdego pliku w grupie
            foreach (var file in files)
            {
                if (file.Id == hub.Id) continue;
                
                yield return new EdgeSnapshot(
                    FromId: hub.Id,
                    ToId:   file.Id,
                    Color:  fileColor,
                    Alpha:  0.12f,
                    Kind:   EdgeKind.Commit);
            }
        }
    }

    // ------------------------------------------------------------------
    // Stylizacja węzłów - kolory zależne od odległości od centrum
    // ------------------------------------------------------------------
    private static List<NodeSnapshot> BuildRadialStyledNodes(IReadOnlyList<NodeSnapshot> nodes)
    {
        return nodes.Select(n =>
        {
            // Oblicz odległość od centrum
            var dist = MathF.Sqrt(n.Position.X * n.Position.X + n.Position.Y * n.Position.Y);

            return n.Kind switch
            {
                // Root - centrum, jasny pomarańczowy z mocnym glow
                NodeKind.Root => n with
                {
                    Radius = 20f,
                    Color = new SysVec4(1f, 0.5f, 0.1f, 1f),
                    Glow = 1.0f
                },

                // Branch - kolory zależne od warstwy
                NodeKind.Branch when dist < 150f => n with
                {
                    // Blisko centrum - pomarańczowo-czerwone
                    Radius = 8f,
                    Color = new SysVec4(1f, 0.3f + (dist / 400f), 0.1f, 1f),
                    Glow = 0.7f - (dist / 300f)
                },
                
                NodeKind.Branch when dist < 300f => n with
                {
                    // Środkowa warstwa - cyan/purple
                    Radius = 7f,
                    Color = new SysVec4(
                        0.2f + (dist / 600f),
                        0.5f + (dist / 1000f),
                        0.8f,
                        1f),
                    Glow = 0.3f
                },
                
                NodeKind.Branch => n with
                {
                    // Daleko - bardziej purplowe
                    Radius = 6f,
                    Color = new SysVec4(0.5f, 0.3f, 0.7f, 1f),
                    Glow = 0.2f
                },

                // File - niebieskie punkty na zewnętrznym pierścieniu
                _ => n with
                {
                    Radius = 4f,
                    Color = new SysVec4(0.3f, 0.6f, 1f, 0.9f), // Cyan-blue
                    Glow = 0.1f
                }
            };
        }).ToList();
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------
    
    private static string GetParentPath(string id)
    {
        int idx = id.LastIndexOf('/');
        return idx < 0 ? string.Empty : id[..idx];
    }

    private void DrawLabels(ISceneSnapshot scene, Matrix3 viewProj)
    {
        var white = new Vector4(1f, 1f, 1f, 0.7f);
        
        // Tylko dla root i branch przy dużym zoomie
        foreach (var node in scene.Nodes.Where(n => n.Kind != NodeKind.File))
        {
            string label = Path.GetFileName(node.Id);
            if (string.IsNullOrEmpty(label)) label = node.Id;
            if (label.Length > 15) label = label[..15];

            var origin = new Vector2(node.Position.X + node.Radius + 2f, node.Position.Y - 3f);
            _text.DrawString(label, origin, LabelScale * 0.7f, white, _textShader, viewProj);
        }
    }

    private void DrawHud(RenderState state, Matrix3 screenMatrix)
    {
        var hud    = state.Hud;
        var white  = new Vector4(1f, 1f, 1f, 1f);
        var yellow = new Vector4(1f, 0.9f, 0.2f, 1f);
        var grey   = new Vector4(0.6f, 0.6f, 0.6f, 1f);

        float x = 12f;
        float y = _viewH - 24f;

        _text.DrawString($"{hud.TimeLabel}  {hud.SpeedLabel}", new(x, y),      1.0f,  white,  _textShader, screenMatrix);
        _text.DrawString($"events {hud.EventsFired}/{hud.TotalEvents}", new(x, y-18), 1.0f,  grey,   _textShader, screenMatrix);

        int bars = (int)(hud.Progress * 40);
        string pbar = "[" + new string('█', bars) + new string('░', 40 - bars) + "]";
        _text.DrawString(pbar, new(x, y - 36), 0.8f, grey, _textShader, screenMatrix);
    }

    private static List<NodeSnapshot> BuildAvatarNodes(IEnumerable<AvatarSnapshot> avatars) =>
        avatars.Select(a => new NodeSnapshot(
            Id:       a.Actor,
            Position: a.Position,
            Radius:   8f,
            Color:    a.Color,
            Glow:     a.ActivityLevel * 0.6f,
            Kind:     NodeKind.File
        )).ToList();

    private Matrix3 BuildScreenMatrix() => new(
        2f / _viewW,  0f,          -1f,
        0f,          -2f / _viewH,  1f,
        0f,           0f,           1f);

    public void Resize(int w, int h)
    {
        _viewW = w;
        _viewH = h;
        GL.Viewport(0, 0, w, h);
    }

    public void Dispose()
    {
        _circleShader.Dispose();
        _edgeShader.Dispose();
        _particleShader.Dispose();
        _textShader.Dispose();
        _circles.Dispose();
        _edges.Dispose();
        _particles.Dispose();
        _text.Dispose();
    }
}