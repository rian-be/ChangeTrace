namespace ChangeTrace.CredentialTrace.Interfaces;

/// <summary>
/// Interface for storing and retrieving authentication sessions (<see cref="AuthSession"/>).
/// </summary>
/// <remarks>
/// This interface defines a persistence mechanism for tokens or credentials:
/// <list type="bullet">
/// <item>Supports saving, retrieving, listing, and removing authentication sessions.</item>
/// <item>All operations are asynchronous and support <see cref="CancellationToken"/> for safe cancellation.</item>
/// <item>Returns <c>null</c> when a requested session is not found (<see cref="GetAsync"/>).</item>
/// <item>Designed to be implemented by classes that persist sessions in memory, file system, database, or secure vault.</item>
/// </list>
/// </remarks>
internal interface ITokenStore
{
    /// <summary>
    /// Saves the specified <paramref name="session"/> for future retrieval.
    /// </summary>
    /// <param name="session">The authentication session to persist.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    Task SaveAsync(AuthSession session, CancellationToken ct = default);

    /// <summary>
    /// Retrieves the authentication session for the given <paramref name="provider"/>.
    /// </summary>
    /// <param name="provider">The provider key for which to retrieve the session.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>
    /// The <see cref="AuthSession"/> if found; otherwise, <c>null</c>.
    /// </returns>
    Task<AuthSession?> GetAsync(string provider, CancellationToken ct = default);

    /// <summary>
    /// Lists all stored authentication sessions.
    /// </summary>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A read-only list of all <see cref="AuthSession"/> objects.</returns>
    Task<IReadOnlyList<AuthSession>> ListAsync(CancellationToken ct = default);

    /// <summary>
    /// Removes the authentication session associated with the given <paramref name="provider"/>.
    /// </summary>
    /// <param name="provider">The provider key of the session to remove.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    Task RemoveAsync(string provider, CancellationToken ct = default);
}
