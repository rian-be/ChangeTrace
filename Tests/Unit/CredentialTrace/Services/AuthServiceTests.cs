using ChangeTrace.CredentialTrace;
using ChangeTrace.CredentialTrace.Interfaces;
using ChangeTrace.CredentialTrace.Services;
using Xunit;

namespace ChangeTrace.Tests.CredentialTrace.Services;

/// <summary>Tests authenticated session reuse, validation, and replacement behavior.</summary>
public sealed class AuthServiceTests
{
    [Fact]
    public async Task FetchSession_ReturnsExistingValidSessionWithoutLogin()
    {
        var existing = AuthSession.Create("codeberg", "valid-token", "rian");
        var store = new InMemoryTokenStore(existing);
        var provider = new TestAuthProvider("codeberg", AuthSession.Create("codeberg", "new-token"), isValid: true);
        var service = new AuthService([provider], store);

        var session = await service.FetchSession("codeberg");

        Assert.Same(existing, session);
        Assert.Equal(0, provider.LoginCount);
    }

    [Fact]
    public async Task FetchSession_ReturnsExistingCodebergSessionWithoutLogin()
    {
        var existing = AuthSession.Create("codeberg", "valid-token", "rian");
        var store = new InMemoryTokenStore(existing);
        var provider = new TestAuthProvider("codeberg", AuthSession.Create("codeberg", "new-token"), isValid: true);
        var service = new AuthService([provider], store);

        var session = await service.FetchSession("codeberg");

        Assert.Same(existing, session);
        Assert.Equal(0, provider.LoginCount);
    }

    [Fact]
    public async Task FetchSession_ReturnsExistingCustomSessionWithoutLogin()
    {
        var existing = AuthSession.Create("custom:example.com", "valid-token", "rian");
        var store = new InMemoryTokenStore(existing);
        var provider = new TestAuthProvider("custom", AuthSession.Create("custom:example.com", "new-token"), isValid: true);
        var service = new AuthService([provider], store);

        var session = await service.FetchSession("custom");

        Assert.Same(existing, session);
        Assert.Equal(0, provider.LoginCount);
    }

    [Fact]
    public async Task FetchSession_RemovesInvalidSessionAndPersistsNewLogin()
    {
        var existing = AuthSession.Create("codeberg", "expired-token", "rian");
        var replacement = AuthSession.Create("codeberg", "fresh-token", "rian");
        var store = new InMemoryTokenStore(existing);
        var provider = new TestAuthProvider("codeberg", replacement, isValid: false);
        var service = new AuthService([provider], store);

        var session = await service.FetchSession("codeberg");

        Assert.Same(replacement, session);
        Assert.Equal(1, provider.LoginCount);
        Assert.Same(replacement, await store.GetAsync("codeberg"));
        Assert.Equal(["codeberg"], store.RemovedProviders);
    }

    private sealed class TestAuthProvider(
        string name,
        AuthSession loginSession,
        bool isValid)
        : IValidatableAuthProvider
    {
        public string Name { get; } = name;
        public bool IsConfigured => true;
        public int LoginCount { get; private set; }

        public Task<AuthSession> LoginAsync(CancellationToken ct = default)
        {
            LoginCount++;
            return Task.FromResult(loginSession);
        }

        public Task<bool> ValidateTokenAsync(string token, CancellationToken ct = default)
            => Task.FromResult(isValid);
    }

    private sealed class InMemoryTokenStore(params AuthSession[] sessions) : ITokenStore
    {
        private readonly Dictionary<string, AuthSession> _sessions = sessions.ToDictionary(
            session => session.Provider,
            StringComparer.OrdinalIgnoreCase);

        public List<string> RemovedProviders { get; } = [];

        public Task SaveAsync(AuthSession session, CancellationToken ct = default)
        {
            _sessions[session.Provider] = session;
            return Task.CompletedTask;
        }

        public Task<AuthSession?> GetAsync(string provider, CancellationToken ct = default)
            => Task.FromResult(_sessions.GetValueOrDefault(provider));

        public Task<IReadOnlyList<AuthSession>> ListAsync(CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<AuthSession>>(_sessions.Values.ToArray());

        public Task RemoveAsync(string provider, CancellationToken ct = default)
        {
            RemovedProviders.Add(provider);
            _sessions.Remove(provider);
            return Task.CompletedTask;
        }
    }
}
