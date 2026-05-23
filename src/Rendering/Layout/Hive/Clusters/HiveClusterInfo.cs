namespace ChangeTrace.Rendering.Layout.Hive.Clusters;

/// <summary>
/// Immutable snapshot describing a hive cluster.
/// </summary>
internal sealed class HiveClusterInfo
{
    /// <summary>
    /// Stable cluster identifier.
    /// </summary>
    public string Id { get; init; } = "";

    /// <summary>
    /// Parent node identifier.
    /// </summary>
    public string ParentId { get; init; } = "";

    /// <summary>
    /// Display label.
    /// </summary>
    public string Label { get; init; } = "";

    /// <summary>
    /// Cluster center position.
    /// </summary>
    public Vec2 Center { get; set; }

    /// <summary>
    /// Cluster render radius.
    /// </summary>
    public float Radius { get; set; }

    /// <summary>
    /// Aggregated recent activity score.
    /// </summary>
    public float ActivityScore { get; init; }

    /// <summary>
    /// Relative cluster importance score.
    /// </summary>
    public float ImportanceScore { get; init; }

    /// <summary>
    /// File identifiers contained inside the cluster.
    /// </summary>
    public IReadOnlyList<string> FileIds { get; init; } =
        [];

    /// <summary>
    /// Suggested label position above cluster.
    /// </summary>
    public Vec2 LabelPosition =>
        Center + new Vec2(0f, -Radius - 36f);

    /// <summary>
    /// Checks whether world position intersects cluster radius.
    /// </summary>
    public bool Contains(
        Vec2 worldPosition,
        float padding)
    {
        Vec2 delta =
            worldPosition - Center;

        float pickRadius =
            Radius + padding;

        return delta.LengthSq <= pickRadius * pickRadius;
    }
}