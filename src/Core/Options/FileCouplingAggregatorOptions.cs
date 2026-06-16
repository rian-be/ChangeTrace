namespace ChangeTrace.Core.Options;

/// <summary>
/// Controls how file coupling pairs are generated from commit bundles.
/// </summary>
/// <param name="MaxFilesPerCommit">
/// Maximum number of files in a commit bundle that will still produce full coupling pairs.
/// Commits above this threshold are skipped to bound quadratic pair generation cost.
/// </param>
internal sealed record FileCouplingAggregatorOptions(
    int MaxFilesPerCommit = 24
);
