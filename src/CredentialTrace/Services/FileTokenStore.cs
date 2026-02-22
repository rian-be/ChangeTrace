using System.Text.Json;
using ChangeTrace.Configuration;
using ChangeTrace.Configuration.Discovery;
using ChangeTrace.CredentialTrace.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.CredentialTrace.Services;

/// <summary>
/// Token store implementation that persists <see cref="AuthSession"/> objects to a local JSON file.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Implements <see cref="ITokenStore"/> to save, retrieve, list, and remove authentication sessions.</item>
/// <item>Uses a JSON file located at <c>%USERPROFILE%/.changetrace/auth.json</c> (or equivalent on non-Windows platforms).</item>
/// <item>Automatically creates the directory if it does not exist.</item>
/// <item>Registered as a singleton via <see cref="AutoRegisterAttribute"/> for dependency injection.</item>
/// <item>Designed to be simple, thread-safe for basic single-process usage, and fully asynchronous.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class FileTokenStore : ITokenStore
{
    private static readonly JsonSerializerOptions Json =
        new() { WriteIndented = true };

    private readonly string _path =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".changetrace",
            "auth.json");

    /// <summary>
    /// Saves or updates the given <see cref="AuthSession"/> in the token store.
    /// </summary>
    /// <param name="session">The session to save.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    public async Task SaveAsync(AuthSession session, CancellationToken ct = default)
    {
        var all = (await ListAsync(ct)).ToDictionary(x => x.Provider);
        all[session.Provider] = session;

        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);

        await File.WriteAllTextAsync(_path,
            JsonSerializer.Serialize(all.Values, Json), ct);
    }

    /// <summary>
    /// Retrieves the <see cref="AuthSession"/> for a specific provider, if it exists.
    /// </summary>
    /// <param name="provider">The provider name.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>The session if found; otherwise, <c>null</c>.</returns>
    public async Task<AuthSession?> GetAsync(string provider, CancellationToken ct = default)
        => (await ListAsync(ct)).FirstOrDefault(x => x.Provider == provider);

    /// <summary>
    /// Lists all stored <see cref="AuthSession"/> objects.
    /// </summary>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A read-only list of all sessions.</returns>
    public async Task<IReadOnlyList<AuthSession>> ListAsync(CancellationToken ct = default)
    {
        if (!File.Exists(_path))
            return [];

        var json = await File.ReadAllTextAsync(_path, ct);
        return JsonSerializer.Deserialize<List<AuthSession>>(json) ?? [];
    }

    /// <summary>
    /// Removes the <see cref="AuthSession"/> for the specified provider from the store.
    /// </summary>
    /// <param name="provider">The provider name.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    public async Task RemoveAsync(string provider, CancellationToken ct = default)
    {
        var all = (await ListAsync(ct)).Where(x => x.Provider != provider);
        await File.WriteAllTextAsync(_path,
            JsonSerializer.Serialize(all, Json), ct);
    }
}