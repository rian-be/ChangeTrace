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

//[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class OpenTkRenderOutput : IRenderOutput, IDisposable
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
    internal bool  ShowLabels   { get; set; } = true;
    internal bool  ShowHud      { get; set; } = true;
    internal float LabelScale   { get; set; } = 0.25f;
    internal float LabelMinZoom { get; set; } = 0.3f;

    private string _fontAtlasPath = "atlas.png";

    public OpenTkRenderOutput() { }

    public void Initialize(int viewportW, int viewportH)
    {
        string fontAtlasPath = "atlas.png";
        _viewW = viewportW;
        _viewH = viewportH;
        _fontAtlasPath = fontAtlasPath;

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

        // 1. Statyczne krawędzie drzewa folder→dziecko (normalne blending)
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        var treeEdges = BuildTreeEdges(scene).ToList();
        _edges.Draw(treeEdges, scene, _edgeShader, viewProj);

        // 2. Dynamiczne krawędzie commitów (additive glow)
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
        _edges.Draw(scene.VisibleEdges().ToList(), scene, _edgeShader, viewProj);

        // 3. Węzły — foldery i pliki (SDF circles)
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        var styledNodes = BuildStyledNodes(scene.Nodes);
        _circles.Draw(styledNodes, _circleShader, viewProj);

        // 4. Bloom na świecących węzłach (additive)
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
        _circles.DrawBloom(styledNodes, _circleShader, viewProj);

        // 5. Avatary deweloperów
        var avatarNodes = BuildAvatarNodes(scene.VisibleAvatars().ToList());
        _circles.Draw(avatarNodes, _circleShader, viewProj);

        // 6. Cząsteczki
        _particles.Draw(scene.Particles, _particleShader, viewProj);

        // 7. Etykiety
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        if (ShowLabels && cam.Zoom >= LabelMinZoom)
            DrawLabels(scene, viewProj);

        // 8. HUD
        if (ShowHud)
            DrawHud(state, BuildScreenMatrix());
    }

    // ------------------------------------------------------------------
    // Tree helpers
    // ------------------------------------------------------------------

    /// <summary>
    /// Buduje linie drzewa katalogów jak w Gource.
    ///
    /// Ponieważ katalogi nie istnieją jako węzły w scenie (Branch = gałęzie gita),
    /// obliczamy pozycje katalogów jako centroidy plików, potem rysujemy linie
    /// parentDir→childDir oraz katalog→plik.
    /// </summary>
    private static IEnumerable<EdgeSnapshot> BuildTreeEdges(ISceneSnapshot scene)
    {
        var fileNodes = scene.Nodes.Where(n => n.Kind == NodeKind.File).ToList();
        if (fileNodes.Count == 0) yield break;

        var dirPositions = BuildVirtualDirPositions(fileNodes);

        var dirColor  = new SysVec4(0.35f, 0.65f, 1.0f, 0.5f);
        var fileColor = new SysVec4(0.55f, 0.55f, 0.75f, 0.35f);

        // Linie parent-katalog → child-katalog
        foreach (var (dirId, _) in dirPositions)
        {
            if (string.IsNullOrEmpty(dirId)) continue;
            var parentId = GetParentPath(dirId);
            if (!dirPositions.ContainsKey(parentId)) continue;

            // Użyj pliku najbliżego centroidu rodzica jako "from"
            // i pliku najbliżego centroidu dziecka jako "to"
            var fromFile = FindFileNearDir(fileNodes, dirPositions[parentId]);
            var toFile   = FindFileNearDir(fileNodes, dirPositions[dirId]);
            if (fromFile == null || toFile == null || fromFile.Id == toFile.Id) continue;

            yield return new EdgeSnapshot(
                FromId: fromFile.Id,
                ToId:   toFile.Id,
                Color:  dirColor,
                Alpha:  dirColor.W,
                Kind:   EdgeKind.Merge);
        }

        // Linie katalog → każdy plik (hub-and-spoke w ramach katalogu)
        var filesByDir = fileNodes.GroupBy(f => GetParentPath(f.Id));
        foreach (var group in filesByDir)
        {
            var siblings = group.ToList();
            if (siblings.Count < 2) continue;

            // Plik najbliższy środkowi grupy = "hub"
            var cx = siblings.Average(f => f.Position.X);
            var cy = siblings.Average(f => f.Position.Y);
            var hub = siblings.OrderBy(f => (f.Position.X - cx) * (f.Position.X - cx)
                                          + (f.Position.Y - cy) * (f.Position.Y - cy)).First();

            foreach (var file in siblings)
            {
                if (file.Id == hub.Id) continue;
                yield return new EdgeSnapshot(
                    FromId: hub.Id,
                    ToId:   file.Id,
                    Color:  fileColor,
                    Alpha:  fileColor.W,
                    Kind:   EdgeKind.Merge);
            }
        }
    }

    private static Dictionary<string, Vec2> BuildVirtualDirPositions(List<NodeSnapshot> files)
    {
        var allDirs = new HashSet<string> { "" };
        foreach (var f in files)
        {
            var p = GetParentPath(f.Id);
            while (!string.IsNullOrEmpty(p))
            {
                allDirs.Add(p);
                p = GetParentPath(p);
            }
        }

        var positions = new Dictionary<string, Vec2>();
        foreach (var dir in allDirs)
        {
            var prefix = string.IsNullOrEmpty(dir) ? "" : dir + "/";
            var relevant = string.IsNullOrEmpty(dir)
                ? files
                : files.Where(f => f.Id.StartsWith(prefix)).ToList();
            if (relevant.Count == 0) continue;
            positions[dir] = new Vec2(
                relevant.Average(f => f.Position.X),
                relevant.Average(f => f.Position.Y));
        }
        return positions;
    }

    private static NodeSnapshot? FindFileNearDir(List<NodeSnapshot> files, Vec2 target)
    {
        if (files.Count == 0) return null;
        return files.OrderBy(f =>
        {
            float dx = f.Position.X - target.X;
            float dy = f.Position.Y - target.Y;
            return dx * dx + dy * dy;
        }).First();
    }

    /// <summary>
    /// Nadaje węzłom właściwe rozmiary i kolory zależne od rodzaju (Root/Branch/File).
    /// </summary>
    private static List<NodeSnapshot> BuildStyledNodes(IReadOnlyList<NodeSnapshot> nodes)
    {
        return nodes.Select(n => n.Kind switch
        {
            NodeKind.Root   => n with { Radius = 18f, Color = new SysVec4(1f, 0.6f, 0.1f, 1f), Glow = 0.8f },
            NodeKind.Branch => n with { Radius = 10f, Color = new SysVec4(0.3f, 0.8f, 0.3f, 1f), Glow = 0.2f },
            _               => n with { Radius =  4f }   // File — mały liść, kolor z snapshot
        }).ToList();
    }

    private static string GetParentPath(string id)
    {
        int idx = id.LastIndexOf('/');
        return idx < 0 ? string.Empty : id[..idx];
    }

    // ------------------------------------------------------------------
    // Labels / HUD (bez zmian)
    // ------------------------------------------------------------------

    private void DrawLabels(ISceneSnapshot scene, Matrix3 viewProj)
    {
        var white = new Vector4(1f, 1f, 1f, 0.8f);
        foreach (var node in scene.Nodes)
        {
            if (node.Kind == NodeKind.File) continue; // rysuj tylko nazwy folderów przy małym zoomie

            string label = Path.GetFileName(node.Id);
            if (string.IsNullOrEmpty(label)) label = node.Id;
            if (label.Length > 24) label = label[..24];

            var origin = new Vector2(node.Position.X + node.Radius + 2f, node.Position.Y - 4f);
            _text.DrawString(label, origin, LabelScale, white, _textShader, viewProj);
        }

        // Nazwy plików tylko przy dużym zoomie
        if (true)
        {
            var fileColor = new Vector4(0.85f, 0.85f, 0.85f, 0.65f);
            foreach (var node in scene.Nodes.Where(n => n.Kind == NodeKind.File))
            {
                string label = Path.GetFileName(node.Id);
                if (string.IsNullOrEmpty(label)) continue;
                if (label.Length > 20) label = label[..20];
                var origin = new Vector2(node.Position.X + 5f, node.Position.Y - 3f);
                _text.DrawString(label, origin, LabelScale * 0.8f, fileColor, _textShader, viewProj);
            }
        }

        var nameColor = new Vector4(1f, 1f, 0.8f, 0.9f);
        foreach (var a in scene.ActiveAvatars())
        {
            var origin = new Vector2(a.Position.X + 10f, a.Position.Y - 5f);
            _text.DrawString(a.Actor, origin, LabelScale * 1.2f, nameColor, _textShader, viewProj);
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
        _text.DrawString($"loop {hud.LoopCount}  ramp {hud.IsRamping}", new(x, y-32), 0.8f,  grey,   _textShader, screenMatrix);

        int bars = (int)(hud.Progress * 40);
        string pbar = "[" + new string('█', bars) + new string('░', 40 - bars) + "]";
        _text.DrawString(pbar, new(x, y - 48), 0.8f, grey, _textShader, screenMatrix);

        float ly = _viewH - 64f;
        foreach (var entry in hud.Leaderboard)
        {
            _text.DrawString($"{entry.Actor,-20} {entry.EventCount}", new(x, ly), 0.85f, yellow, _textShader, screenMatrix);
            ly -= 16f;
        }
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