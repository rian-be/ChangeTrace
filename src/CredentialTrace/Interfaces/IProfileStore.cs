namespace ChangeTrace.CredentialTrace.Interfaces;

/// <summary>
/// Generic store for <see cref="IProfile"/> objects.
/// </summary>
/// <typeparam name="T">Type of profile, must implement <see cref="IProfile"/>.</typeparam>
/// <remarks>
/// <list type="bullet">
/// <item>Supports basic CRUD: save, delete, get by ID, get by name, list all.</item>
/// <item>Can be implemented with different storage backends: file, memory, or database.</item>
/// <item>Designed for dependency injection and async usage.</item>
/// </list>
/// </remarks>
internal interface IProfileStore<T>
    where T : class, IProfile
{
    /// <summary>Save or update profile.</summary>
    Task SaveAsync(T profile, CancellationToken ct = default);

    /// <summary>Delete profile by ID.</summary>
    Task DeleteAsync(Ulid id, CancellationToken ct = default);

    /// <summary>Get profile by ID. Returns null if not found.</summary>
    Task<T?> GetAsync(Ulid id, CancellationToken ct = default);

    /// <summary>Get profile by name. Returns null if not found.</summary>
    Task<T?> GetByNameAsync(string name, CancellationToken ct = default);

    /// <summary>List all profiles.</summary>
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);
}