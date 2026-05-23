namespace ChangeTrace.Rendering.Layout.Hive.Core;

/// <summary>
/// Tunable parameters controlling hive layout generation and scaling.
/// </summary>
internal sealed record HiveLayoutOptions
{
    /// <summary>
    /// Synthetic root node identifier used by the layout tree.
    /// </summary>
    public string RootId { get; init; } = "__repo_root__";

    /// <summary>
    /// Interpolation factor used during animated transitions.
    /// </summary>
    public float AnimationSmoothness { get; init; } = 0.28f;

    /// <summary>
    /// Maximum allowed layout radius before normalization.
    /// </summary>
    public float MaxLayoutRadius { get; init; } = 4200f;

    /// <summary>
    /// Lowest scale allowed during layout fitting.
    /// </summary>
    public float MinFitScale { get; init; } = 0.02f;

    /// <summary>
    /// Scale delta required before snapping nodes to targets.
    /// </summary>
    public float FitSnapThreshold { get; init; } = 0.08f;

    /// <summary>
    /// Minimum radius reserved for the root region.
    /// </summary>
    public float MinRootRadius { get; init; } = 620f;

    /// <summary>
    /// Minimum radius reserved for child directory regions.
    /// </summary>
    public float MinChildRadius { get; init; } = 420f;

    /// <summary>
    /// Extra spacing added between sibling regions.
    /// </summary>
    public float SiblingGap { get; init; } = 180f;

    /// <summary>
    /// Target spacing between file nodes on a ring.
    /// </summary>
    public float FileSpacing { get; init; } = 68f;

    /// <summary>
    /// Radius of the first file ring.
    /// </summary>
    public float FileRingStart { get; init; } = 110f;

    /// <summary>
    /// Distance between consecutive file rings.
    /// </summary>
    public float FileRingGap { get; init; } = 88f;

    /// <summary>
    /// Minimum number of files placed on a ring.
    /// </summary>
    public int MinFilesPerRing { get; init; } = 5;

    /// <summary>
    /// Maximum number of files placed on a ring.
    /// </summary>
    public int MaxFilesPerRing { get; init; } = 10;

    /// <summary>
    /// File count threshold for switching to a heavy cluster layout.
    /// </summary>
    public int HeavyFileFolderThreshold { get; init; } = 120;

    /// <summary>
    /// Preferred number of files per heavy cluster.
    /// </summary>
    public int HeavyFileClusterSize { get; init; } = 18;

    /// <summary>
    /// Horizontal spacing between heavy clusters.
    /// </summary>
    public float HeavyClusterSpacingX { get; init; } = 560f;

    /// <summary>
    /// Vertical spacing between heavy clusters.
    /// </summary>
    public float HeavyClusterSpacingY { get; init; } = 520f;

    /// <summary>
    /// Base distance from parent node to heavy cluster regions.
    /// </summary>
    public float HeavyRegionDistance { get; init; } = 900f;

    /// <summary>
    /// Minimum number of columns used by heavy cluster grids.
    /// </summary>
    public int HeavyMinColumns { get; init; } = 4;

    /// <summary>
    /// Maximum number of columns used by heavy cluster grids.
    /// </summary>
    public int HeavyMaxColumns { get; init; } = 18;

    /// <summary>
    /// Random positional variation applied to heavy clusters.
    /// </summary>
    public float HeavyClusterJitter { get; init; } = 20f;

    /// <summary>
    /// Weight applied to recent activity during cluster scoring.
    /// </summary>
    public float GlowActivityWeight { get; init; } = 2.0f;

    /// <summary>
    /// Weight applied to cluster size during importance scoring.
    /// </summary>
    public float SizeImportanceWeight { get; init; } = 0.35f;

    /// <summary>
    /// Extra padding used during cluster hit testing.
    /// </summary>
    public float ClusterPickPadding { get; init; } = 48f;
}