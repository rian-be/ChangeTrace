using ChangeTrace.CredentialTrace.Interfaces;

namespace ChangeTrace.Tests.TestDoubles;

/// <summary>Inmemory profile store test double keyed by profile id.</summary>
internal sealed class InMemoryProfileStore<T>(params T[] profiles) : IProfileStore<T>
    where T : class, IProfile
{
    private readonly Dictionary<Ulid, T> _profiles = profiles.ToDictionary(profile => profile.Id);

    /// <summary>Saves or replaces profile by id.</summary>
    public Task SaveAsync(T profile, CancellationToken ct = default)
    {
        _profiles[profile.Id] = profile;
        return Task.CompletedTask;
    }

    /// <summary>Deletes profile by id when present.</summary>
    public Task DeleteAsync(Ulid id, CancellationToken ct = default)
    {
        _profiles.Remove(id);
        return Task.CompletedTask;
    }

    /// <summary>Returns the profile stored for the id, if present.</summary>
    public Task<T?> GetAsync(Ulid id, CancellationToken ct = default)
        => Task.FromResult(_profiles.GetValueOrDefault(id));

    /// <summary>Finds profile by case insensitive name.</summary>
    public Task<T?> GetByNameAsync(string name, CancellationToken ct = default)
        => Task.FromResult(_profiles.Values.FirstOrDefault(profile =>
            profile.Name.Equals(name, StringComparison.OrdinalIgnoreCase)));

    /// <summary>Returns all profiles currently stored in memory.</summary>
    public Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
        => Task.FromResult<IEnumerable<T>>(_profiles.Values);
}
