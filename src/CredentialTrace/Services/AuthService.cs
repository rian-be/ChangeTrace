using System.Collections.Concurrent;
using ChangeTrace.Configuration;
using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core.Results;
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
    
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Returns existing session or performs login if missing.
    /// Intended for normal application operations.
    /// </summary>
    public async Task<AuthSession> FetchSession(string provider, CancellationToken ct = default)
    {
        var p = GetProvider(provider);

        var gate = _locks.GetOrAdd(provider, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(ct);
        try
        {
            var existing = await store.GetAsync(provider, ct);

            if (existing is not null)
            {
                var validation = await ValidateSession(p, existing, ct);
                if (validation.IsSuccess)
                    return existing;

                await store.RemoveAsync(provider, ct);
            }

            return await PerformLogin(p, ct);
        }
        finally
        {
            gate.Release();
        }
    }
    
    /// <summary>
    /// Logs out from the specified provider and removes its persisted session.
    /// </summary>
    /// <param name="provider">The name of the authentication provider.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    public Task LogoutSession(string provider, CancellationToken ct = default)
        => store.RemoveAsync(provider, ct);

    /// <summary>
    /// Lists all stored authentication sessions.
    /// </summary>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A read-only list of all <see cref="AuthSession"/> objects.</returns>
    public Task<IReadOnlyList<AuthSession>> ListProviders(CancellationToken ct = default)
        => store.ListAsync(ct);

    /// <summary>
    /// Retrieves the stored <see cref="AuthSession"/> for the specified provider, if it exists.
    /// </summary>
    /// <param name="provider">The name of the authentication provider.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>The <see cref="AuthSession"/> if found; otherwise, <c>null</c>.</returns>
    public Task<AuthSession?> GetSession(string provider, CancellationToken ct = default)
        => store.GetAsync(provider, ct);
    
    private IAuthProvider GetProvider(string provider) => !_providers.TryGetValue(provider, out var p)
        ? throw new InvalidOperationException($"Provider '{provider}' not registered") : p;
    
    private async Task<AuthSession> PerformLogin(IAuthProvider provider, CancellationToken ct)
    {
        var session = await provider.LoginAsync(ct);
        await store.SaveAsync(session, ct);
        return session;
    }
    
    private static async Task<Result> ValidateSession(IAuthProvider provider, AuthSession session, CancellationToken ct)
    {
        try
        {
            if (provider is not IValidatableAuthProvider validatable) return Result.Success();
            var ok = await validatable.ValidateTokenAsync(session.AccessToken, ct);
            return ok 
                ? Result.Success() 
                : Result.Failure("Token validation failed or expired");

        }
        catch (Exception ex)
        {
            return Result.Failure("Exception during token validation", ex);
        }
    }
}