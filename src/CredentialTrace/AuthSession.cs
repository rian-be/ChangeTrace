namespace ChangeTrace.CredentialTrace;

/// <summary>
/// Represents an authenticated session for a specific provider.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Encapsulates the access token, provider name, optional username, and creation timestamp.</item>
/// <item>Immutable record type for safe transport and storage.</item>
/// <item>Used by AuthService and TokenStore to persist and manage sessions.</item>
/// <item>Creation timestamp (<see cref="CreatedAt"/>) indicates when the session was obtained.</item>
/// </list>
/// </remarks>
internal sealed record AuthSession(
    string Provider,
    string AccessToken,
    string? Username,
    DateTimeOffset CreatedAt);