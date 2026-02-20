namespace ChangeTrace.CredentialTrace.Interfaces;

/// <summary>
/// Unified service for managing authentication sessions across multiple providers.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Supports login and logout for different authentication providers.</item>
/// <item>Persists and retrieves <see cref="AuthSession"/> objects via a backing <see cref="ITokenStore"/>.</item>
/// <item>Designed for dependency injection; intended as a singleton service.</item>
/// <item>Provides methods to list all sessions or retrieve a session by provider name.</item>
/// </list>
/// </remarks>
internal interface IAuthService
{
    /// <summary>
    /// Logs in to the specified provider and returns the resulting <see cref="AuthSession"/>.
    /// </summary>
    /// <param name="provider">Authentication provider name, e.g., "github" or "gitlab".</param>
    /// <param name="ct">Cancellation token for the login operation.</param>
    /// <returns>Authenticated <see cref="AuthSession"/>.</returns>
    Task<AuthSession> FetchSession(string provider, CancellationToken ct = default);

    /// <summary>
    /// Logs out from the specified provider and removes its persisted session.
    /// </summary>
    /// <param name="provider">Authentication provider name.</param>
    /// <param name="ct">Cancellation token for the logout operation.</param>
    Task LogoutSession(string provider, CancellationToken ct = default);

    /// <summary>
    /// Lists all stored authentication sessions.
    /// </summary>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>Read-only list of <see cref="AuthSession"/> objects.</returns>
    Task<IReadOnlyList<AuthSession>> ListProviders(CancellationToken ct = default);
    
    /// <summary>
    /// Retrieves the stored <see cref="AuthSession"/> for the specified provider, if it exists.
    /// </summary>
    /// <param name="provider">Authentication provider name.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns><see cref="AuthSession"/> if found; otherwise, <c>null</c>.</returns>
    Task<AuthSession?> GetSession(string provider, CancellationToken ct = default);
}