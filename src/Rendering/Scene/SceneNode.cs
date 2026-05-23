using System.Numerics;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Scene.Graph;

namespace ChangeTrace.Rendering.Scene;

/// <summary>
/// Shared scene node identifiers.
/// </summary>
internal static class SceneIds
{
    internal const string Root = "__repo_root__";
    internal const string RootLabel = "Repository";
}

/// <summary>
/// Mutable scene graph node.
/// </summary>
internal sealed class SceneNode
{
    private readonly NodeFlyweight _flyweight;

    private float _forceX;
    private float _forceY;

    internal SceneNode(
        string id,
        NodeKind kind,
        Vec2 position,
        Vector4? color = null)
    {
        Id = SceneNodeIds.Normalize(id, kind);
        _flyweight = NodeFlyweightFactory.ForKind(kind);

        Position = position;
        HomePosition = position;
        Color = color ?? new Vector4(1f);

        int lastSlash = Id.LastIndexOf('/');

        ParentId = SceneNodeIds.ResolveParentId(Id, kind);
        Label = ResolveLabel(Id, kind, lastSlash);
        Extension = ResolveExtension(kind, Label);
        CachedColor = ResolveCachedColor(kind, Id);
    }

    /// <summary>
    /// Shared immutable node data.
    /// </summary>
    internal NodeFlyweight Flyweight =>
        _flyweight;

    /// <summary>
    /// Stable scene node identifier.
    /// </summary>
    internal string Id { get; }

    /// <summary>
    /// Display label.
    /// </summary>
    internal string Label { get; }

    /// <summary>
    /// File extension for file nodes.
    /// </summary>
    internal string Extension { get; }

    /// <summary>
    /// Indicates whether this node has children.
    /// </summary>
    internal bool IsParent { get; set; }

    /// <summary>
    /// Node type.
    /// </summary>
    internal NodeKind Kind =>
        _flyweight.Kind;

    /// <summary>
    /// Current world position.
    /// </summary>
    internal Vec2 Position { get; set; }

    /// <summary>
    /// Layout home position.
    /// </summary>
    internal Vec2 HomePosition { get; set; }

    /// <summary>
    /// Current layout velocity.
    /// </summary>
    internal Vec2 Velocity { get; set; }

    /// <summary>
    /// Accumulated layout force.
    /// </summary>
    internal Vec2 Force
    {
        get => new(_forceX, _forceY);
        set
        {
            _forceX = value.X;
            _forceY = value.Y;
        }
    }

    /// <summary>
    /// Node mass used by layout simulation.
    /// </summary>
    internal float Mass =>
        _flyweight.Mass;

    /// <summary>
    /// Node radius used by rendering and layout.
    /// </summary>
    internal float Radius =>
        _flyweight.Radius;

    /// <summary>
    /// Current node color.
    /// </summary>
    internal Vector4 Color { get; set; }

    /// <summary>
    /// Current glow intensity.
    /// </summary>
    internal float Glow { get; set; }

    /// <summary>
    /// The last author touching this node.
    /// </summary>
    internal string? LastAuthor { get; set; }

    /// <summary>
    /// The last commit touching this node.
    /// </summary>
    internal string? LastCommit { get; set; }

    /// <summary>
    /// Parent node identifier.
    /// </summary>
    internal string? ParentId { get; set; }

    /// <summary>
    /// Prevents layout from moving this node.
    /// </summary>
    internal bool Pinned { get; set; }

    /// <summary>
    /// Cached semantic node color.
    /// </summary>
    internal Vector4 CachedColor { get; }

    /// <summary>
    /// Resolves node display label.
    /// </summary>
    private static string ResolveLabel(
        string id,
        NodeKind kind,
        int lastSlash)
    {
        if (kind == NodeKind.Root)
            return SceneIds.RootLabel;

        return lastSlash >= 0
            ? id[(lastSlash + 1)..]
            : id;
    }

    /// <summary>
    /// Resolves file extension for file nodes.
    /// </summary>
    private static string ResolveExtension(
        NodeKind kind,
        string label)
    {
        if (kind != NodeKind.File)
            return "";

        int lastDot =
            label.LastIndexOf('.');

        return lastDot >= 0
            ? label[lastDot..].ToLowerInvariant()
            : "";
    }

    /// <summary>
    /// Resolves cached semantic color.
    /// </summary>
    private static Vector4 ResolveCachedColor(
        NodeKind kind,
        string id)
    {
        return kind switch
        {
            NodeKind.Root => new Vector4(1f, 0.8f, 0.3f, 1f),
            NodeKind.Branch => new Vector4(0.6f, 0.6f, 0.6f, 0.5f),
            _ => Colors.ColorPalette.ForFilePath(id)
        };
    }
}