namespace ChangeTrace.CredentialTrace.Dto;

/// <summary>
/// Represents the response returned by an OAuth device authorization endpoint.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Encapsulates all fields required for device-based authentication flows.</item>
/// <item><c>device_code</c> — the code used by the application to poll the token endpoint.</item>
/// <item><c>user_code</c> — the code the user enters at the verification URL to authorize the device.</item>
/// <item><c>verification_uri</c> — the URL the user must visit to complete authorization.</item>
/// <item><c>expires_in</c> — the lifetime of the <c>device_code</c> in seconds.</item>
/// <item><c>interval</c> — the minimum interval (in seconds) that the client should wait between polling attempts.</item>
/// <item>Designed as an immutable record for safe transport and easy deserialization.</item>
/// </list>
/// </remarks>
internal sealed record DeviceCodeResponse(
    string device_code,
    string user_code,
    string verification_uri,
    int expires_in,
    int interval);