using System.Numerics;
using ChangeTrace.Rendering.Enums;

namespace ChangeTrace.Rendering.Scene;

/// <summary>
/// Represents a mutable node instance in the scene graph.
/// </summary>
/// <remarks>
/// Stores per-node spatial state, cached display metadata, and flyweight-backed static rendering properties.
/// </remarks>
internal sealed class SceneNode
{
    private readonly NodeFlyweight _flyweight;

    /// <summary>
    /// Gets flyweight containing shared node properties.
    /// </summary>
    internal NodeFlyweight Flyweight => _flyweight;

    /// <summary>
    /// Gets unique scene node identifier.
    /// </summary>
    internal string Id { get; }

    /// <summary>
    /// Gets display label derived from node identifier.
    /// </summary>
    internal string Label { get; }

    /// <summary>
    /// Gets file extension for file nodes.
    /// </summary>
    internal string Extension { get; } = "";

    /// <summary>
    /// Gets or sets whether a node acts as a parent in scene hierarchy.
    /// </summary>
    internal bool IsParent { get; set; }

    /// <summary>
    /// Gets node kind.
    /// </summary>
    internal NodeKind Kind => _flyweight.Kind;

    /// <summary>
    /// Gets or sets current world position.
    /// </summary>
    internal Vec2 Position { get; set; }

    /// <summary>
    /// Gets or sets a preferred layout position.
    /// </summary>
    internal Vec2 HomePosition { get; set; }

    /// <summary>
    /// Gets or sets the current layout velocity.
    /// </summary>
    internal Vec2 Velocity { get; set; }

    private float _forceX;
    private float _forceY;

    /// <summary>
    /// Gets or sets accumulated layout force.
    /// </summary>
    internal Vec2 Force
    {
        get => new Vec2(_forceX, _forceY);
        set { _forceX = value.X; _forceY = value.Y; }
    }

    /// <summary>
    /// Adds force to the node using atomic float accumulation.
    /// </summary>
    /// <param name="f">Force vector to add.</param>
    public void AddForce(Vec2 f)
    {
        AddFloat(ref _forceX, f.X);
        AddFloat(ref _forceY, f.Y);
    }

    /// <summary>
    /// Atomically adds floating-point value to a specified storage location.
    /// </summary>
    /// <param name="location">Storage location to update.</param>
    /// <param name="value">Value to add.</param>
    private static void AddFloat(ref float location, float value)
    {
        do
        {
            var current = location;
            float updated = current + value;
            float original = Interlocked.CompareExchange(
                ref location,
                updated,
                current);

            if (BitConverter.SingleToInt32Bits(original) ==
                BitConverter.SingleToInt32Bits(current))
            {
                break;
            }
        }
        while (true);
    }

    /// <summary>
    /// Gets node mass used by layout simulation.
    /// </summary>
    internal float Mass => _flyweight.Mass;

    /// <summary>
    /// Gets node render radius.
    /// </summary>
    internal float Radius => _flyweight.Radius;

    /// <summary>
    /// Gets or sets the current render color.
    /// </summary>
    internal Vector4 Color { get; set; }

    /// <summary>
    /// Gets or sets emissive glow intensity.
    /// </summary>
    internal float Glow { get; set; }

    /// <summary>
    /// Gets or sets the last author associated with this node.
    /// </summary>
    internal string? LastAuthor { get; set; }

    /// <summary>
    /// Gets or sets the last commit identifier associated with this node.
    /// </summary>
    internal string? LastCommit { get; set; }

    /// <summary>
    /// Gets or sets parent node identifier.
    /// </summary>
    internal string? ParentId { get; set; }

    /// <summary>
    /// Gets or sets whether the layout fixes the node position.
    /// </summary>
    internal bool Pinned { get; set; }

    /// <summary>
    /// Gets or sets child index inside parent groups.
    /// </summary>
    internal int ChildIndex { get; set; }

    /// <summary>
    /// Gets or sets sibling index inside layout groups.
    /// </summary>
    internal int SiblingIndex { get; set; }

    /// <summary>
    /// Gets stable color derived from the node kind or file path.
    /// </summary>
    internal Vector4 CachedColor { get; }

    /// <summary>
    /// Creates a scene node with identifier, kind, position, and optional color override.
    /// </summary>
    /// <param name="id">Unique scene node identifier.</param>
    /// <param name="kind">Node kind.</param>
    /// <param name="position">Initial world position.</param>
    /// <param name="color">Optional initial render color.</param>
    internal SceneNode(string id, NodeKind kind, Vec2 position, Vector4? color = null)
    {
        Id = id;
        _flyweight = NodeFlyweightFactory.ForKind(kind);
        Position = position;
        HomePosition = position;
        Color = color ?? new Vector4(1f, 1f, 1f, 1f);

        int lastSlash = id.LastIndexOf('/');
        ParentId = lastSlash >= 0 ? id.Substring(0, lastSlash) : null;
        Label = lastSlash >= 0 ? id.Substring(lastSlash + 1) : id;

        if (kind == NodeKind.File)
        {
            int lastDot = Label.LastIndexOf('.');
            Extension = lastDot >= 0 ? Label.Substring(lastDot).ToLowerInvariant() : "";
        }

        if (kind == NodeKind.Root)
            CachedColor = new Vector4(1, 0.8f, 0.3f, 1);
        else if (kind == NodeKind.Branch)
            CachedColor = new Vector4(0.6f, 0.6f, 0.6f, 0.5f);
        else
            CachedColor = Colors.ColorPalette.ForFilePath(id);
    }
}