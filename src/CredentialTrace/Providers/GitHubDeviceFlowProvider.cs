using System.Net.Http.Headers;
using System.Net.Http.Json;
using ChangeTrace.Configuration;
using ChangeTrace.CredentialTrace.Dto;
using ChangeTrace.CredentialTrace.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace ChangeTrace.CredentialTrace.Providers;

/// <summary>
/// Authentication provider for GitHub using the OAuth device authorization flow.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Implements <see cref="IAuthProvider"/> for GitHub-specific login logic.</item>
/// <item>Handles the device flow by requesting a device code, showing instructions to the user, and polling for an access token.</item>
/// <item>Uses <c>HttpClient</c> for HTTP requests and <see cref="DeviceCodeResponse"/> / <see cref="AccessTokenResponse"/> DTOs for serialization.</item>
/// <item>Displays user instructions and code via <see cref="Spectre.Console"/> panels.</item>
/// <item>Registered as a singleton via <see cref="AutoRegisterAttribute"/> for DI.</item>
/// <item>Supports cancellation via <see cref="CancellationToken"/>.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class GitHubDeviceFlowProvider(HttpClient http) : IAuthProvider, IValidatableAuthProvider
{
    private const string ClientId = "Ov23liZADv3yvX37gzcw";

    /// <summary>
    /// Gets the unique name of this authentication provider.
    /// </summary>
    public string Name => "github";

    /// <summary>
    /// Performs login via GitHub device flow and returns the resulting <see cref="AuthSession"/>.
    /// </summary>
    /// <param name="ct">Cancellation token to cancel the login operation.</param>
    /// <returns>The authenticated <see cref="AuthSession"/>.</returns>
    public async Task<AuthSession> LoginAsync(CancellationToken ct = default)
    {
        var device = await RequestDeviceCode(ct);
        ShowInstructions(device);

        var token = await PollAccessToken(device, ct);

        return new AuthSession(
            Provider: Name,
            AccessToken: token.access_token,
            Username: null,
            CreatedAt: DateTimeOffset.UtcNow);
    }

    public async Task<bool> ValidateTokenAsync(string token, CancellationToken ct = default)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req.Headers.UserAgent.ParseAdd("ChangeTraceCLI");

        var res = await http.SendAsync(req, ct);
        return res.IsSuccessStatusCode;
    }
    
    /// <summary>
    /// Requests a device code from GitHub for the device flow.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="DeviceCodeResponse"/> containing codes and verification URI.</returns>
    private async Task<DeviceCodeResponse> RequestDeviceCode(CancellationToken ct)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "https://github.com/login/device/code")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = ClientId,
                ["scope"] = "repo read:user"
            })
        };
        
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var res = await http.SendAsync(request, ct);
        res.EnsureSuccessStatusCode();

        var device = await res.Content.ReadFromJsonAsync<DeviceCodeResponse>(cancellationToken: ct);
        return device ?? throw new Exception("Invalid GitHub response");
    }

    /// <summary>
    /// Displays user instructions for completing the device authorization in the browser.
    /// </summary>
    /// <param name="d">The <see cref="DeviceCodeResponse"/> containing verification information.</param>
    private static void ShowInstructions(DeviceCodeResponse d)
    {
        AnsiConsole.WriteLine();
        
        var urlPanel = new Panel($"[bold yellow] Open in browser:[/]\n[underline blue]{d.verification_uri}[/]")
        {
            Border = BoxBorder.Double,
            Padding = new Padding(1, 1),
            Header = new PanelHeader("[bold]GitHub Device Flow[/]", Justify.Center),
            Expand = true
        };
        AnsiConsole.Write(urlPanel);

        AnsiConsole.WriteLine();
        
        var codePanel = new Panel($"[bold green] Enter this code:[/]\n[white bold]{d.user_code}[/]")
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

    /// <summary>
    /// Polls GitHub for the access token until the user authorizes the device or an error occurs.
    /// </summary>
    /// <param name="device">The <see cref="DeviceCodeResponse"/> to poll for.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The <see cref="AccessTokenResponse"/> containing the access token.</returns>
    /// <exception cref="OperationCanceledException">If the operation is canceled.</exception>
    /// <exception cref="Exception">If the token response contains an error other than "authorization_pending" or "slow_down".</exception>
    private async Task<AccessTokenResponse> PollAccessToken(DeviceCodeResponse device, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(device.interval), ct);

            var request = new HttpRequestMessage(HttpMethod.Post, "https://github.com/login/oauth/access_token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["client_id"] = ClientId,
                    ["device_code"] = device.device_code,
                    ["grant_type"] = "urn:ietf:params:oauth:grant-type:device_code"
                })
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var res = await http.SendAsync(request, ct);
            res.EnsureSuccessStatusCode();

            var token = await res.Content.ReadFromJsonAsync<AccessTokenResponse>(cancellationToken: ct)
                        ?? throw new Exception("Invalid token response");

            if (!string.IsNullOrEmpty(token.access_token))
                return token;

            switch (token.error)
            {
                case "authorization_pending":
                    continue;
                case "slow_down":
                    await Task.Delay(2000, ct);
                    continue;
                default:
                    throw new Exception($"OAuth error: {token.error} {token.error_description}");
            }
        }

        throw new OperationCanceledException();
    }
}
