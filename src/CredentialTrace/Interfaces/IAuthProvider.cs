namespace ChangeTrace.CredentialTrace.Interfaces;

/// <summary>
/// Represents an authentication provider capable of performing login operations.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Defines a unique <see cref="Name"/> to identify the provider (e.g., "github", "gitlab").</item>
/// <item>Provides an asynchronous <see cref="LoginAsync"/> method to perform authentication and return an <see cref="AuthSession"/>.</item>
/// <item>Designed to support dependency injection and be implemented by multiple providers.</item>
/// <item>Login may involve interactive or non-interactive flows depending on the provider.</item>
/// </list>
/// </remarks>
internal interface IAuthProvider
{
    /// <summary>
    /// Gets the unique name of the authentication provider.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Performs the login operation asynchronously and returns an <see cref="AuthSession"/>.
    /// </summary>
    /// <param name="ct">Cancellation token to cancel the login operation.</param>
    /// <returns>The authenticated <see cref="AuthSession"/>.</returns>
    Task<AuthSession> LoginAsync(CancellationToken ct = default);
}