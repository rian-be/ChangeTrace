using ChangeTrace.Configuration;
using ChangeTrace.CredentialTrace.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.CredentialTrace.Services;

/// <summary>
/// Service responsible for handling authentication using registered <see cref="IAuthProvider"/> instances
/// and managing persisted <see cref="AuthSession"/> objects via <see cref="ITokenStore"/>.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Automatically registered as a singleton via <see cref="AutoRegisterAttribute"/>.</item>
/// <item>Delegates login/logout operations to the appropriate <see cref="IAuthProvider"/>.</item>
/// <item>Persists authentication sessions using <see cref="ITokenStore"/> after successful login.</item>
/// <item>Provides methods to list and retrieve stored sessions without exposing persistence details.</item>
/// <item>Designed to support multiple providers identified by a case-insensitive name.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class AuthService(
    IEnumerable<IAuthProvider> providers,
    ITokenStore store) : IAuthService
{
    private readonly Dictionary<string, IAuthProvider> _providers =
        providers.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Logs in to the specified provider and stores the resulting <see cref="AuthSession"/>.
    /// </summary>
    /// <param name="provider">The name of the authentication provider.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>The <see cref="AuthSession"/> obtained from the provider.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the provider is not registered.</exception>
    public async Task<AuthSession> LoginAsync(string provider, CancellationToken ct = default)
    {
        if (!_providers.TryGetValue(provider, out var p))
            throw new InvalidOperationException($"Provider '{provider}' not registered");

        var session = await p.LoginAsync(ct);
        Console.WriteLine(session.AccessToken);
        await store.SaveAsync(session, ct);

        return session;
    }

    /// <summary>
    /// Logs out from the specified provider and removes its persisted session.
    /// </summary>
    /// <param name="provider">The name of the authentication provider.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    public Task LogoutAsync(string provider, CancellationToken ct = default)
        => store.RemoveAsync(provider, ct);

    /// <summary>
    /// Lists all stored authentication sessions.
    /// </summary>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A read-only list of all <see cref="AuthSession"/> objects.</returns>
    public Task<IReadOnlyList<AuthSession>> ListAsync(CancellationToken ct = default)
        => store.ListAsync(ct);

    /// <summary>
    /// Retrieves the stored <see cref="AuthSession"/> for the specified provider, if it exists.
    /// </summary>
    /// <param name="provider">The name of the authentication provider.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>The <see cref="AuthSession"/> if found; otherwise, <c>null</c>.</returns>
    public Task<AuthSession?> GetAsync(string provider, CancellationToken ct = default)
        => store.GetAsync(provider, ct);
}