namespace ChangeTrace.Rendering.Snapshots;

/// <summary>
/// Axis aligned bounding box defined by minimum and maximum coordinates.
/// </summary>
internal readonly record struct Bounds(Vec2 Min, Vec2 Max)
{
    /// <summary>
    /// The center point of bounds.
    /// </summary>
    internal Vec2 Center => new((Min.X + Max.X) / 2f, (Min.Y + Max.Y) / 2f);

    /// <summary>
    /// The width and height vector of bounds.
    /// </summary>
    internal Vec2 Size => Max - Min;

    /// <summary>
    /// The width of bounds.
    /// </summary>
    internal float Width => Max.X - Min.X;

    /// <summary>
    /// The height of bounds.
    /// </summary>
    internal float Height => Max.Y - Min.Y;

    /// <summary>
    /// Determines whether point is contained within bounds.
    /// </summary>
    /// <param name="point">Point to test.</param>
    /// <returns>True if point is inside bounds; otherwise false.</returns>
    internal bool Contains(Vec2 point) =>
        point.X >= Min.X && point.X <= Max.X &&
        point.Y >= Min.Y && point.Y <= Max.Y;

    /// <summary>
    /// Expands bounds by a uniform margin.
    /// </summary>
    /// <param name="margin">Margin to inflate bounds by.</param>
    /// <returns>New <see cref="Bounds"/> expanded by margin.</returns>
    internal Bounds Inflate(float margin) =>
        new(Min - new Vec2(margin, margin), Max + new Vec2(margin, margin));
}