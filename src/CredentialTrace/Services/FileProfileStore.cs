using System.Collections.Concurrent;
using System.Text.Json;
using ChangeTrace.Configuration.Converters;
using ChangeTrace.Configuration.Discovery;
using ChangeTrace.CredentialTrace.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.CredentialTrace.Services;

/// <summary>
/// File backed store for <see cref="IProfile"/> objects.
/// </summary>
/// <typeparam name="TProfile">Type of profile, must implement <see cref="IProfile"/>.</typeparam>
/// <remarks>
/// <list type="bullet">
/// <item>Persists profiles as JSON files under AppContext base directory.</item>
/// <item>Caches loaded profiles in memory for fast access.</item>
/// <item>Supports saving, deleting, and querying profiles by ID or name.</item>
/// <item>Registered as singleton via <see cref="AutoRegisterAttribute"/>.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class FileProfileStore<TProfile> : IProfileStore<TProfile>
    where TProfile : class, IProfile
{
    private readonly string _dataDir;
    private readonly ConcurrentDictionary<Ulid, TProfile> _cache = new();
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance and loads existing profiles from disk.
    /// </summary>
    public FileProfileStore()
    {
        _dataDir = Path.Combine(AppContext.BaseDirectory, "profiles", typeof(TProfile).Name);
        Directory.CreateDirectory(_dataDir);

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new UlidJsonConverter() }
        };

        LoadAll();
    }

    private void LoadAll()
    {
        var files = Directory.Exists(_dataDir) 
            ? Directory.GetFiles(_dataDir, "*.json") 
            : [];

        foreach (var file in files)
        {
            try
            {
                var json = File.ReadAllText(file);
                var profile = JsonSerializer.Deserialize<TProfile>(json, _jsonOptions);
                if (profile != null)
                    _cache[profile.Id] = profile;
            }
            catch
            {
                // ignored
            }
        }
    }

    private void SaveToFile(TProfile profile)
    {
        var path = Path.Combine(_dataDir, $"{profile.Id}.json");
        var json = JsonSerializer.Serialize(profile, _jsonOptions);
        File.WriteAllText(path, json);
    }

    private void DeleteFile(Ulid id)
    {
        var path = Path.Combine(_dataDir, $"{id}.json");
        if (File.Exists(path))
            File.Delete(path);
    }

    /// <inheritdoc/>
    public Task SaveAsync(TProfile profile, CancellationToken ct = default)
    {
        _cache[profile.Id] = profile;
        SaveToFile(profile);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task DeleteAsync(Ulid id, CancellationToken ct = default)
    {
        _cache.TryRemove(id, out _);
        DeleteFile(id);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<TProfile?> GetAsync(Ulid id, CancellationToken ct = default)
    {
        _cache.TryGetValue(id, out var profile);
        return Task.FromResult(profile);
    }

    /// <inheritdoc/>
    public Task<TProfile?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Task.FromResult<TProfile?>(null);

        var profile = _cache.Values
            .FirstOrDefault(p => p.Name?.Equals(name, StringComparison.OrdinalIgnoreCase) == true);

        return Task.FromResult(profile);
    }

    /// <inheritdoc/>
    public Task<IEnumerable<TProfile>> GetAllAsync(CancellationToken ct = default)
        => Task.FromResult<IEnumerable<TProfile>>(_cache.Values);
}