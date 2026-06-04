using ChangeTrace.CredentialTrace;
using ChangeTrace.CredentialTrace.Interfaces;
using ChangeTrace.CredentialTrace.Services;
using Xunit;

namespace ChangeTrace.Tests.CredentialTrace.Services;

/// <summary>Tests authenticated session reuse, validation, and replacement behavior.</summary>
public sealed class AuthServiceTests
{
    /// <summary>FetchSession returns an existing valid session without invoking provider login.</summary>
    [Fact]
    public async Task FetchSession_ReturnsExistingValidSessionWithoutLogin()
    {
        var existing = AuthSession.Create("github", "valid-token", "rian");
        var store = new InMemoryTokenStore(existing);
        var provider = new TestAuthProvider("github", AuthSession.Create("github", "new-token"), isValid: true);
        var service = new AuthService([provider], store);

        var session = await service.FetchSession("GitHub");

        Assert.Same(existing, session);
        Assert.Equal(0, provider.LoginCount);
    }

    /// <summary>FetchSession removes an invalid stored session and persists the provider login result.</summary>
    [Fact]
    public async Task FetchSession_RemovesInvalidSessionAndPersistsNewLogin()
    {
        var existing = AuthSession.Create("github", "expired-token", "rian");
        var replacement = AuthSession.Create("github", "fresh-token", "rian");
        var store = new InMemoryTokenStore(existing);
        var provider = new TestAuthProvider("github", replacement, isValid: false);
        var service = new AuthService([provider], store);

        var session = await service.FetchSession("github");

        Assert.Same(replacement, session);
        Assert.Equal(1, provider.LoginCount);
        Assert.Same(replacement, await store.GetAsync("github"));
        Assert.Equal(["github"], store.RemovedProviders);
    }

    /// <summary>Test auth provider that records login calls and returns a configurable validation result.</summary>
    private sealed class TestAuthProvider(
        string name,
        AuthSession loginSession,
        bool isValid)
        : IValidatableAuthProvider
    {
        /// <summary>Provider name matched by AuthService.</summary>
        public string Name { get; } = name;

        /// <summary>Number of times LoginAsync was invoked.</summary>
        public int LoginCount { get; private set; }

        /// <summary>Returns the configured login session and increments LoginCount.</summary>
        public Task<AuthSession> LoginAsync(CancellationToken ct = default)
        {
            LoginCount++;
            return Task.FromResult(loginSession);
        }

        /// <summary>Returns the configured validation result for any token.</summary>
        public Task<bool> ValidateTokenAsync(string token, CancellationToken ct = default)
            => Task.FromResult(isValid);
    }

    /// <summary>In-memory token store used to assert AuthService persistence behavior.</summary>
    private sealed class InMemoryTokenStore(params AuthSession[] sessions) : ITokenStore
    {
        private readonly Dictionary<string, AuthSession> _sessions = sessions.ToDictionary(
            session => session.Provider,
            StringComparer.OrdinalIgnoreCase);

        /// <summary>Providers removed through RemoveAsync, in call order.</summary>
        public List<string> RemovedProviders { get; } = [];

        /// <summary>Saves or replaces <paramref name="session"/> by provider.</summary>
        public Task SaveAsync(AuthSession session, CancellationToken ct = default)
        {
            _sessions[session.Provider] = session;
            return Task.CompletedTask;
        }

        /// <summary>Returns the session stored for provider, if present.</summary>
        public Task<AuthSession?> GetAsync(string provider, CancellationToken ct = default)
            => Task.FromResult(_sessions.GetValueOrDefault(provider));

        /// <summary>Returns all sessions currently stored in memory.</summary>
        public Task<IReadOnlyList<AuthSession>> ListAsync(CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<AuthSession>>(_sessions.Values.ToArray());

        /// <summary>Removes a provider session and records the provider name.</summary>
        public Task RemoveAsync(string provider, CancellationToken ct = default)
        {
            RemovedProviders.Add(provider);
            _sessions.Remove(provider);
            return Task.CompletedTask;
        }
    }
}
