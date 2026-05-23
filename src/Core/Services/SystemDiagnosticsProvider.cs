using System.Collections.Concurrent;
using System.Diagnostics;
using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Core.Services;

[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class SystemDiagnosticsProvider : IDiagnosticsProvider
{
    private readonly Process _currentProcess = Process.GetCurrentProcess();

    private DateTime _lastRefresh = DateTime.MinValue;
    private void EnsureRefreshed()
    {
        if ((DateTime.Now - _lastRefresh).TotalMilliseconds > 1000)
        {
            _currentProcess.Refresh();
            _lastRefresh = DateTime.Now;
        }
    }

    public MemoryMetrics GetMemoryMetrics()
    {
        EnsureRefreshed();
        return new MemoryMetrics(
            ManagedMb: (float)(GC.GetTotalMemory(false) / 1024.0 / 1024.0),
            WorkingSetMb: (float)(_currentProcess.WorkingSet64 / 1024.0 / 1024.0),
            PrivateBytesMb: (float)(_currentProcess.PrivateMemorySize64 / 1024.0 / 1024.0)
        );
    }

    public int[] GetGcCollections()
    {
        return new[] { 
            GC.CollectionCount(0), 
            GC.CollectionCount(1), 
            GC.CollectionCount(2) 
        };
    }

    public RuntimeMetrics GetRuntimeMetrics()
    {
        EnsureRefreshed();
        return new RuntimeMetrics(
            ThreadCount: _currentProcess.Threads.Count,
            HandleCount: _currentProcess.HandleCount,
            CpuUsagePercentage: GetCpuUsage(),
            UpTimeSeconds: (DateTime.Now - _currentProcess.StartTime).TotalSeconds
        );
    }

    private double _lastCpuCheck = 0;
    private TimeSpan _lastCpuTime = TimeSpan.Zero;
    private double GetCpuUsage()
    {
        var now = (DateTime.Now - _currentProcess.StartTime).TotalSeconds;
        var cpuTime = _currentProcess.TotalProcessorTime;
        
        if (_lastCpuCheck > 0)
        {
            var deltaWall = now - _lastCpuCheck;
            var deltaCpu = (cpuTime - _lastCpuTime).TotalSeconds;
            _lastCpuTime = cpuTime;
            _lastCpuCheck = now;
            return Math.Max(0, (deltaCpu / deltaWall) * 100.0 / Environment.ProcessorCount);
        }

        _lastCpuTime = cpuTime;
        _lastCpuCheck = now;
        return 0;
    }

    private readonly ConcurrentDictionary<string, double> _customMetrics = new();

    public IReadOnlyDictionary<string, double> GetCustomMetrics() => _customMetrics;

    public void RecordMetric(string key, double value) => _customMetrics[key] = value;

    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, int>> _events = new();
    private readonly ConcurrentDictionary<string, (DateTime Time, IReadOnlyList<KeyValuePair<string, int>> Result)> _topCache = new();

    public void RecordEvent(string category, string label)
    {
        var cat = _events.GetOrAdd(category, _ => new ConcurrentDictionary<string, int>());
        cat.AddOrUpdate(label, 1, (_, old) => old + 1);
    }

    public IReadOnlyList<KeyValuePair<string, int>> GetTopEvents(string category, int count)
    {
        if (!_events.TryGetValue(category, out var cat))
            return Array.Empty<KeyValuePair<string, int>>();

        if (_topCache.TryGetValue(category, out var cached) && (DateTime.Now - cached.Time).TotalMilliseconds < 500)
            return cached.Result;

        var result = cat.OrderByDescending(kv => kv.Value).Take(count).ToList();
        _topCache[category] = (DateTime.Now, result);
        return result;
    }
}
