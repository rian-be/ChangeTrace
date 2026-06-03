namespace ChangeTrace.Benchmarks;

/// <summary>
/// Shared BenchmarkDotNet category names used for focused benchmark runs.
/// </summary>
internal static class BenchmarkCategories
{
    public const string Core = "Core";
    public const string Git = "Git";
    public const string Export = "Export";
    public const string Timeline = "Timeline";
    public const string Serialization = "Serialization";
    public const string Aggregation = "Aggregation";
    public const string FileCoupling = "FileCoupling";
}
