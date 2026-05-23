using System.Numerics;

namespace ChangeTrace.Core.Diagnostics;

/// <summary>
/// Defines a provider for runtime performance and resource metrics.
/// Decoupled from rendering and simulation logic.
/// </summary>
public interface IDiagnosticsProvider
{
    /// <summary>
    /// Gets current memory usage metrics.
    /// </summary>
    MemoryMetrics GetMemoryMetrics();

    /// <summary>
    /// Gets GC collection counts for all generations.
    /// </summary>
    int[] GetGcCollections();

    /// <summary>
    /// Gets runtime execution metrics (CPU, threads, etc).
    /// </summary>
    RuntimeMetrics GetRuntimeMetrics();

    /// <summary>
    /// Gets any custom metrics indexed by key.
    /// </summary>
    IReadOnlyDictionary<string, double> GetCustomMetrics();

    /// <summary>
    /// Records a custom metric value.
    /// </summary>
    void RecordMetric(string key, double value);

    /// <summary>
    /// Records an occurrence of an event in a category.
    /// </summary>
    void RecordEvent(string category, string label);

    /// <summary>
    /// Gets top labels in a category by occurrence count.
    /// </summary>
    IReadOnlyList<KeyValuePair<string, int>> GetTopEvents(string category, int count);
}

public record MemoryMetrics(
    float ManagedMb,
    float WorkingSetMb,
    float PrivateBytesMb
);

public record RuntimeMetrics(
    int ThreadCount,
    int HandleCount,
    double CpuUsagePercentage,
    double UpTimeSeconds
);
