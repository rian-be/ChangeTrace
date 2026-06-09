using System.Collections.Concurrent;
using ChangeTrace.Configuration;
using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core.Results;
using ChangeTrace.CredentialTrace.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.CredentialTrace.Services;

/// <summary>
/// Authentication session service.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class AuthService(
    IEnumerable<IAuthProvider> providers,
    ITokenStore store) : IAuthService
{
    private readonly Dictionary<string, IAuthProvider> _providers =
        providers.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
    
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Returns an existing session or performs login.
    /// </summary>
    public async Task<AuthSession> FetchSession(string provider, CancellationToken ct = default)
    {
        var canonicalProvider = NormalizeProviderName(provider);
        var p = GetProvider(canonicalProvider);

        var gate = _locks.GetOrAdd(canonicalProvider, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(ct);
        try
        {
            var existing = await GetStoredSessionAsync(canonicalProvider, ct);

            if (existing is not null)
            {
                var validation = await ValidateSession(p, existing, ct);
                if (validation.IsSuccess)
                    return existing;

                await store.RemoveAsync(canonicalProvider, ct);
            }

            return await PerformLogin(p, ct);
        }
        finally
        {
            gate.Release();
        }
    }
    
    /// <summary>
    /// Removes a stored session.
    /// </summary>
    public async Task LogoutSession(string provider, CancellationToken ct = default)
    {
        var canonicalProvider = NormalizeProviderName(provider);
        if (string.Equals(canonicalProvider, "custom", StringComparison.OrdinalIgnoreCase))
        {
            var sessions = await store.ListAsync(ct);
            foreach (var session in sessions.Where(session => session.Provider.StartsWith("custom:", StringComparison.OrdinalIgnoreCase)))
                await store.RemoveAsync(session.Provider, ct);

            await store.RemoveAsync(canonicalProvider, ct);
            return;
        }

        await store.RemoveAsync(canonicalProvider, ct);
    }

    /// <summary>
    /// Lists stored sessions.
    /// </summary>
    public Task<IReadOnlyList<AuthSession>> ListProviders(CancellationToken ct = default)
        => store.ListAsync(ct);

    /// <summary>
    /// Retrieves a stored session.
    /// </summary>
    public async Task<AuthSession?> GetSession(string provider, CancellationToken ct = default)
    {
        var canonicalProvider = NormalizeProviderName(provider);
        return await GetStoredSessionAsync(canonicalProvider, ct);
    }
    
    private IAuthProvider GetProvider(string provider) => !_providers.TryGetValue(provider, out var p)
        ? throw new InvalidOperationException($"Provider '{provider}' not registered") : p;

    /// <summary>
    /// Normalizes provider names.
    /// </summary>
    private static string NormalizeProviderName(string provider)
    {
        var normalized = provider.Trim().ToLowerInvariant();

        return normalized switch
        {
            var value when value.StartsWith("custom:", StringComparison.OrdinalIgnoreCase) => "custom",
            "custom" => "custom",
            _ => normalized
        };
    }
    
    private async Task<AuthSession> PerformLogin(IAuthProvider provider, CancellationToken ct)
    {
        var session = await provider.LoginAsync(ct);
        await store.SaveAsync(session, ct);
        return session;
    }

    /// <summary>
    /// Gets a stored session for a provider.
    /// </summary>
    private async Task<AuthSession?> GetStoredSessionAsync(string canonicalProvider, CancellationToken ct)
    {
        var session = await store.GetAsync(canonicalProvider, ct);
        if (session is not null)
            return session;

        if (string.Equals(canonicalProvider, "custom", StringComparison.OrdinalIgnoreCase))
        {
            var sessions = await store.ListAsync(ct);
            return sessions.FirstOrDefault(session => session.Provider.StartsWith("custom:", StringComparison.OrdinalIgnoreCase));
        }
        return null;
    }
    
    /// <summary>
    /// Validates a stored session.
    /// </summary>
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
