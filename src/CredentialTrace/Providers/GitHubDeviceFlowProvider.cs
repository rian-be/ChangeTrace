using System.Net.Http.Headers;
using ChangeTrace.Configuration.Discovery;
using ChangeTrace.CredentialTrace.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.CredentialTrace.Providers;

/// <summary>
/// Authentication provider for GitHub using the OAuth device authorization flow.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class GitHubDeviceFlowProvider(HttpClient http) : BaseDeviceFlowProvider(http)
{
    /// <summary>
    /// Gets the provider name.
    /// </summary>
    public override string Name => "github";

    /// <summary>
    /// Gets the display name used in logs and prompts.
    /// </summary>
    protected override string DisplayName => "GitHub";

    /// <summary>
    /// Gets the client id environment variable name.
    /// </summary>
    protected override string ClientIdEnvironmentVariable => "CHANGETRACE_GITHUB_CLIENT_ID";

    /// <summary>
    /// Gets the device code endpoint.
    /// </summary>
    protected override string DeviceCodeEndpoint => "https://github.com/login/device/code";

    /// <summary>
    /// Gets the token endpoint.
    /// </summary>
    protected override string TokenEndpoint => "https://github.com/login/oauth/access_token";

    /// <summary>
    /// Gets the requested OAuth scopes.
    /// </summary>
    protected override string Scope => "repo read:user";

    /// <summary>
    /// Gets the missing client id error message.
    /// </summary>
    protected override string MissingClientIdMessage =>
        "GitHub device flow requires CHANGETRACE_GITHUB_CLIENT_ID to be set to the OAuth application client id.";

    /// <summary>
    /// Validates a GitHub access token.
    /// </summary>
    public override async Task<bool> ValidateTokenAsync(string token, CancellationToken ct = default)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req.Headers.UserAgent.ParseAdd("ChangeTraceCLI");

        var res = await Http.SendAsync(req, ct);
        return res.IsSuccessStatusCode;
    }
}
