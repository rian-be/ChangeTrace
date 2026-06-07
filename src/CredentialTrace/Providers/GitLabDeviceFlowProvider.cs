using System.Net.Http.Headers;
using ChangeTrace.Configuration.Discovery;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.CredentialTrace.Providers;

/// <summary>
/// Authentication provider for GitLab using the OAuth device authorization flow.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class GitLabDeviceFlowProvider(HttpClient http) : BaseDeviceFlowProvider(http)
{
    /// <summary>
    /// Gets the provider name.
    /// </summary>
    public override string Name => "gitlab";

    /// <summary>
    /// Gets the display name used in logs and prompts.
    /// </summary>
    protected override string DisplayName => "GitLab";

    /// <summary>
    /// Gets the client id environment variable name.
    /// </summary>
    protected override string ClientIdEnvironmentVariable => "CHANGETRACE_GITLAB_CLIENT_ID";

    /// <summary>
    /// Gets the device code endpoint.
    /// </summary>
    protected override string DeviceCodeEndpoint => $"{ResolveBaseUrl()}/oauth/authorize_device";

    /// <summary>
    /// Gets the token endpoint.
    /// </summary>
    protected override string TokenEndpoint => $"{ResolveBaseUrl()}/oauth/token";

    /// <summary>
    /// Gets the requested OAuth scopes.
    /// </summary>
    protected override string Scope => "read_api read_user";

    /// <summary>
    /// Gets the missing client id error message.
    /// </summary>
    protected override string MissingClientIdMessage =>
        "GitLab device flow requires CHANGETRACE_GITLAB_CLIENT_ID (or legacy CHANGETRACE_GITLAB_DEVICE_FLOW__CLIENTID) to be set to the OAuth application client id.";

    /// <summary>
    /// Validates a GitLab access token.
    /// </summary>
    public override async Task<bool> ValidateTokenAsync(string token, CancellationToken ct = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{ResolveBaseUrl()}/api/v4/user");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.UserAgent.ParseAdd("ChangeTrace");

        using var response = await Http.SendAsync(request, ct);
        return response.IsSuccessStatusCode;
    }

    private string ResolveBaseUrl()
    {
        var baseUrl = Environment.GetEnvironmentVariable("CHANGETRACE_GITLAB_BASE_URL")
            ?? Environment.GetEnvironmentVariable("CHANGETRACE_GITLAB_DEVICE_FLOW__BASEURL");
        if (string.IsNullOrWhiteSpace(baseUrl))
            return "https://gitlab.com";

        return baseUrl.Trim().TrimEnd('/');
    }
}
