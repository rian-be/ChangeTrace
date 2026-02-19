namespace ChangeTrace.CredentialTrace.Dto;

/// <summary>
/// Represents the response returned by an OAuth or token-based authentication provider.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Encapsulates all standard fields returned from a token endpoint.</item>
/// <item><c>access_token</c> — the issued access token used for authentication.</item>
/// <item><c>token_type</c> — the type of token (typically "bearer").</item>
/// <item><c>scope</c> — the scope of access granted by the token.</item>
/// <item><c>error</c> — an optional error code if the request failed.</item>
/// <item><c>error_description</c> — a human-readable description of the error, if any.</item>
/// <item>Designed as an immutable record for safe and simple data transport.</item>
/// </list>
/// </remarks>
internal sealed record AccessTokenResponse(
    string access_token,
    string token_type,
    string scope,
    string error,
    string error_description);