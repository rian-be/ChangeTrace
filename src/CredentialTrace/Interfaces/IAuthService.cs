namespace ChangeTrace.CredentialTrace.Interfaces;

/// <summary>
/// Provides a unified service for managing authentication sessions across multiple providers.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Supports logging in and out for different authentication providers.</item>
/// <item>Persists and retrieves <see cref="AuthSession"/> objects using a backing <see cref="ITokenStore"/>.</item>
/// <item>Designed for dependency injection and to be implemented as a singleton service.</item>
/// <item>Provides methods to list all sessions or retrieve a specific session by provider name.</item>
/// </list>
/// </remarks>
internal interface IAuthService
{
    /// <summary>
    /// Logs in to the specified provider and returns the resulting <see cref="AuthSession"/>.
    /// </summary>
    /// <param name="provider">The name of the authentication provider (e.g., "github", "gitlab").</param>
    /// <param name="ct">Cancellation token to cancel the login operation.</param>
    /// <returns>The authenticated <see cref="AuthSession"/>.</returns>
    Task<AuthSession> LoginAsync(string provider, CancellationToken ct = default);

    /// <summary>
    /// Logs out from the specified provider and removes its persisted session.
    /// </summary>
    /// <param name="provider">The name of the authentication provider.</param>
    /// <param name="ct">Cancellation token to cancel the logout operation.</param>
    Task LogoutAsync(string provider, CancellationToken ct = default);

    /// <summary>
    /// Lists all stored authentication sessions.
    /// </summary>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A read-only list of all <see cref="AuthSession"/> objects.</returns>
    Task<IReadOnlyList<AuthSession>> ListAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Retrieves the stored <see cref="AuthSession"/> for the specified provider, if it exists.
    /// </summary>
    /// <param name="provider">The name of the authentication provider.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>The <see cref="AuthSession"/> if found; otherwise, <c>null</c>.</returns>
    Task<AuthSession?> GetAsync(string provider, CancellationToken ct = default);
}
