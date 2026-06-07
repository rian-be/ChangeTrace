using System.Net.Http.Headers;
using System.Net.Http.Json;
using ChangeTrace.CredentialTrace.Dto;
using ChangeTrace.CredentialTrace.Interfaces;
using Spectre.Console;

namespace ChangeTrace.CredentialTrace.Providers;

/// <summary>
/// Base class for OAuth device flow providers.
/// </summary>
internal abstract class BaseDeviceFlowProvider(HttpClient http) : IValidatableAuthProvider
{
    /// <summary>
    /// Gets the provider name.
    /// </summary>
    protected HttpClient Http { get; } = http;

    /// <summary>
    /// Gets the provider name.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets the display name used in logs and prompts.
    /// </summary>
    protected abstract string DisplayName { get; }

    /// <summary>
    /// Gets the environment variable that contains the client id.
    /// </summary>
    protected abstract string ClientIdEnvironmentVariable { get; }

    /// <summary>
    /// Gets the device code endpoint.
    /// </summary>
    protected abstract string DeviceCodeEndpoint { get; }

    /// <summary>
    /// Gets the access token endpoint.
    /// </summary>
    protected abstract string TokenEndpoint { get; }

    /// <summary>
    /// Gets the requested OAuth scopes.
    /// </summary>
    protected abstract string Scope { get; }

    /// <summary>
    /// Gets the error message shown when the client id is missing.
    /// </summary>
    protected abstract string MissingClientIdMessage { get; }

    /// <summary>
    /// Validates an access token for the provider.
    /// </summary>
    public abstract Task<bool> ValidateTokenAsync(string token, CancellationToken ct = default);

    /// <summary>
    /// Performs the device flow login.
    /// </summary>
    public async Task<AuthSession> LoginAsync(CancellationToken ct = default)
    {
        var clientId = ResolveClientId();
        var device = await RequestDeviceCodeAsync(clientId, ct);
        ShowInstructions(device);
        var token = await PollAccessTokenAsync(clientId, device, ct);

        return AuthSession.Create(Name, token.access_token, null);
    }

    /// <summary>
    /// Requests a device code from the provider.
    /// </summary>
    protected async Task<DeviceCodeResponse> RequestDeviceCodeAsync(
        string clientId,
        CancellationToken ct)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, DeviceCodeEndpoint)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = clientId,
                ["scope"] = Scope
            })
        };

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await Http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var device = await response.Content.ReadFromJsonAsync<DeviceCodeResponse>(cancellationToken: ct);
        return device ?? throw new InvalidOperationException($"Invalid {DisplayName} device authorization response.");
    }

    /// <summary>
    /// Polls the token endpoint until authorization completes or fails.
    /// </summary>
    protected async Task<AccessTokenResponse> PollAccessTokenAsync(
        string clientId,
        DeviceCodeResponse device,
        CancellationToken ct)
    {
        var pollingInterval = TimeSpan.FromSeconds(Math.Max(1, device.interval));

        while (!ct.IsCancellationRequested)
        {
            await Task.Delay(pollingInterval, ct);

            var request = new HttpRequestMessage(HttpMethod.Post, TokenEndpoint)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["client_id"] = clientId,
                    ["device_code"] = device.device_code,
                    ["grant_type"] = "urn:ietf:params:oauth:grant-type:device_code"
                })
            };

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await Http.SendAsync(request, ct);
            var token = await response.Content.ReadFromJsonAsync<AccessTokenResponse>(cancellationToken: ct)
                        ?? throw new InvalidOperationException($"Invalid {DisplayName} token response.");

            if (!string.IsNullOrWhiteSpace(token.access_token))
                return token;

            switch (token.error)
            {
                case "authorization_pending":
                    continue;
                case "slow_down":
                    pollingInterval += TimeSpan.FromSeconds(2);
                    continue;
                case "access_denied":
                    throw new InvalidOperationException($"{DisplayName} authorization was denied.");
                case "expired_token":
                    throw new InvalidOperationException($"{DisplayName} device code expired.");
                default:
                    if (response.IsSuccessStatusCode)
                        throw new InvalidOperationException($"The {DisplayName} token response did not contain an access token.");

                    throw new InvalidOperationException(
                        $"{DisplayName} token polling failed ({(int)response.StatusCode}): {token.error} {token.error_description}".Trim());
            }
        }

        throw new OperationCanceledException();
    }

    /// <summary>
    /// Displays the device authorization instructions.
    /// </summary>
    protected virtual void ShowInstructions(DeviceCodeResponse device)
    {
        AnsiConsole.WriteLine();

        var urlPanel = new Panel($"[bold yellow] Open in browser:[/]\n[underline blue]{device.verification_uri}[/]")
        {
            Border = BoxBorder.Double,
            Padding = new Padding(1, 1),
            Header = new PanelHeader($"[bold]{DisplayName} Device Flow[/]", Justify.Center),
            Expand = true
        };
        AnsiConsole.Write(urlPanel);

        AnsiConsole.WriteLine();

        var codePanel = new Panel($"[bold green] Enter this code:[/]\n[white bold]{device.user_code}[/]")
        {
            Border = BoxBorder.Rounded,
            Padding = new Padding(1, 1),
            Expand = true
        };
        AnsiConsole.Write(codePanel);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[italic grey] Waiting for authorization...[/]");
        AnsiConsole.WriteLine();
    }

    private string ResolveClientId()
    {
        var clientId = Environment.GetEnvironmentVariable(ClientIdEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(clientId))
            return clientId.Trim();

        throw new InvalidOperationException(MissingClientIdMessage);
    }
}
