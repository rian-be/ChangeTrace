namespace ChangeTrace.CredentialTrace;

/// <summary>
/// Represents an authenticated session for a specific provider.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Encapsulates the access token, provider name, optional username, ULID identifier, and creation timestamp.</item>
/// <item>Immutable record type for safe transport and storage.</item>
/// <item>Used by AuthService and TokenStore to persist and manage sessions.</item>
/// <item>Creation timestamp (<see cref="CreatedAt"/>) indicates when the session was obtained.</item>
/// </list>
/// </remarks>
internal sealed record AuthSession(
    string Provider,
    string AccessToken,
    string? Username,
    Ulid Id,
    DateTimeOffset CreatedAt
)
{
    /// <summary>
    /// Factory method to create  AuthSession with generated ULID.
    /// </summary>
    public static AuthSession Create(string provider, string accessToken, string? username = null)
        => new(
            Provider: provider,
            AccessToken: accessToken,
            Username: username,
            Id: Ulid.NewUlid(),
            CreatedAt: DateTimeOffset.UtcNow
        );
}