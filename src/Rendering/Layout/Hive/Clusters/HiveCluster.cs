using ChangeTrace.Rendering.Scene;

namespace ChangeTrace.Rendering.Layout.Hive.Clusters;

/// <summary>
/// Runtime cluster used by hive layout grouping.
/// </summary>
internal sealed class HiveCluster
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
    /// Cluster file nodes.
    /// </summary>
    public List<SceneNode> Files { get; init; } =
        [];

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
    public float ActivityScore { get; set; }

    /// <summary>
    /// Relative cluster importance score.
    /// </summary>
    public float ImportanceScore { get; set; }

    /// <summary>
    /// Converts runtime cluster into an immutable info snapshot.
    /// </summary>
    public HiveClusterInfo ToInfo()
    {
        return new HiveClusterInfo
        {
            Id = Id,
            ParentId = ParentId,
            Label = Label,
            Center = Center,
            Radius = Radius,
            ActivityScore = ActivityScore,
            ImportanceScore = ImportanceScore,
            FileIds = Files
                .Select(x => x.Id)
                .ToArray()
        };
    }
}