using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using ChangeTrace.Configuration.Discovery;
using ChangeTrace.CredentialTrace.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace ChangeTrace.CredentialTrace.Providers.AuthorizationCodePkce;

/// <summary>
/// OAuth provider for Codeberg.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class CodebergOAuthProvider(HttpClient http) : IValidatableAuthProvider
{
    private HttpClient Http { get; } = http;

    private const int RedirectPort = 8765;
    private const string RedirectPath = "/callback";
    private const string Scope = "read:user";
    private const string AuthorizeEndpointPath = "/login/oauth/authorize";
    private const string TokenEndpointPath = "/login/oauth/access_token";
    private const string UserEndpointPath = "/api/v1/user";
    private static readonly Uri RedirectUri = new($"http://127.0.0.1:{RedirectPort}{RedirectPath}");

    private static readonly CodebergPreset[] Presets =
    [
        new(
            DisplayName: "Codeberg",
            BaseUrl: "https://codeberg.org",
            ClientIdEnvironmentVariable: "CHANGETRACE_CODEBERG_CLIENT_ID")
    ];

    private static readonly string[] LegacyClientIdEnvironmentVariables =
    [
        "CHANGETRACE_FORGEJO_CODEBERG_CLIENT_ID"
    ];

    private const string ClientSecretEnvironmentVariable = "CHANGETRACE_CODEBERG_CLIENT_SECRET";
    private const string LegacyClientSecretEnvironmentVariable = "CHANGETRACE_FORGEJO_CODEBERG_CLIENT_SECRET";

    /// <summary>
    /// PKCE verifier/challenge pair.
    /// </summary>
    private readonly record struct PkcePair(string Verifier, string Challenge);

    /// <summary>
    /// Codeberg OAuth preset.
    /// </summary>
    private sealed record CodebergPreset(
        string DisplayName,
        string BaseUrl,
        string ClientIdEnvironmentVariable)
    {
        public string AuthorizeEndpoint => $"{BaseUrl}{AuthorizeEndpointPath}";
        public string TokenEndpoint => $"{BaseUrl}{TokenEndpointPath}";
        public string UserEndpoint => $"{BaseUrl}{UserEndpointPath}";
    }

    private sealed record AccessTokenResponse(
        [property: JsonPropertyName("access_token")] string? AccessToken);

    /// <summary>
    /// Gets the unique provider name.
    /// </summary>
    public string Name => "codeberg";

    /// <summary>
    /// Gets whether the provider is configured.
    /// </summary>
    public bool IsConfigured =>
        Presets.Any(p => !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(p.ClientIdEnvironmentVariable))) ||
        LegacyClientIdEnvironmentVariables.Any(name => !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(name)));

    /// <summary>
    /// Performs the Codeberg OAuth login flow.
    /// </summary>
    public async Task<AuthSession> LoginAsync(CancellationToken ct = default)
    {
        var preset = Presets[0];
        var clientId = ResolveClientId(preset);
        var pkce = CreatePkcePair();
        var state = CreateState();
        var authorizationUrl = BuildAuthorizationUrl(preset, clientId, pkce.Challenge, state);

        ShowLoginInstructions(preset, authorizationUrl);

        using var listener = CreateListener();
        var codeTask = WaitForAuthorizationCodeAsync(listener, state, ct);

        TryOpenBrowser(authorizationUrl);

        var code = await codeTask;
        var accessToken = await ExchangeCodeAsync(preset, clientId, code, pkce.Verifier, ct);

        return AuthSession.Create(Name, accessToken);
    }

    /// <summary>
    /// Validates a Codeberg access token.
    /// </summary>
    public async Task<bool> ValidateTokenAsync(string token, CancellationToken ct = default)
    {
        try
        {
            return await ValidateTokenAsync(Presets[0], token, ct);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Resolves the OAuth client id.
    /// </summary>
    private static string ResolveClientId(CodebergPreset preset)
    {
        var clientId = Environment.GetEnvironmentVariable(preset.ClientIdEnvironmentVariable)
                       ?? Environment.GetEnvironmentVariable("CHANGETRACE_FORGEJO_CODEBERG_CLIENT_ID");
        if (!string.IsNullOrWhiteSpace(clientId))
            return clientId.Trim();

        throw new InvalidOperationException(
            $"{preset.DisplayName} login requires {preset.ClientIdEnvironmentVariable} to be set to the OAuth application client id.");
    }

    /// <summary>
    /// Resolves the OAuth client secret.
    /// </summary>
    private static string? ResolveClientSecret()
    {
        var clientSecret = Environment.GetEnvironmentVariable(ClientSecretEnvironmentVariable)
            ?? Environment.GetEnvironmentVariable(LegacyClientSecretEnvironmentVariable);

        return string.IsNullOrWhiteSpace(clientSecret)
            ? null
            : clientSecret.Trim();
    }

    /// <summary>
    /// Builds the authorization URL.
    /// </summary>
    private static string BuildAuthorizationUrl(
        CodebergPreset preset,
        string clientId,
        string codeChallenge,
        string state)
    {
        var query = new Dictionary<string, string>
        {
            ["client_id"] = clientId,
            ["response_type"] = "code",
            ["redirect_uri"] = RedirectUri.ToString(),
            ["scope"] = Scope,
            ["state"] = state,
            ["code_challenge"] = codeChallenge,
            ["code_challenge_method"] = "S256"
        };

        return $"{preset.AuthorizeEndpoint}?{string.Join("&", query.Select(pair => $"{pair.Key}={Uri.EscapeDataString(pair.Value)}"))}";
    }

    /// <summary>
    /// Shows login instructions.
    /// </summary>
    private static void ShowLoginInstructions(CodebergPreset preset, string authorizationUrl)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(
            new Panel(
                    $"[bold]{preset.DisplayName} login[/]\n" +
                    $"[grey]Redirect URI:[/] [white]{RedirectUri}[/]\n" +
                    $"[grey]Authorization URL:[/] [blue underline]{authorizationUrl}[/]")
                .Border(BoxBorder.Double)
                .Padding(1, 1)
                .Header("[bold]Codeberg OAuth[/]"));
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[italic grey]Waiting for authorization...[/]");
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Creates the local callback listener.
    /// </summary>
    private static TcpListener CreateListener()
        => new(System.Net.IPAddress.Loopback, RedirectPort);

    /// <summary>
    /// Waits for the authorization callback.
    /// </summary>
    private static async Task<string> WaitForAuthorizationCodeAsync(
        TcpListener listener,
        string expectedState,
        CancellationToken ct)
    {
        listener.Start();

        try
        {
            using var client = await listener.AcceptTcpClientAsync(ct);
            await using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.ASCII, leaveOpen: true);

            var requestLine = await reader.ReadLineAsync(ct);
            if (string.IsNullOrWhiteSpace(requestLine))
                throw new InvalidOperationException("Codeberg authorization callback was empty.");

            while (!string.IsNullOrEmpty(await reader.ReadLineAsync(ct)))
            {
            }

            var target = requestLine.Split(' ', 3)[1];
            var uri = target.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                ? new Uri(target)
                : new Uri($"http://127.0.0.1:{RedirectPort}{target}");

            var query = ParseQuery(uri.Query);
            var code = query.GetValueOrDefault("code");
            var state = query.GetValueOrDefault("state");
            var error = query.GetValueOrDefault("error");

            await WriteResponseAsync(stream, error is null ? "Login completed. You can close this tab." : error, ct);

            if (!string.IsNullOrWhiteSpace(error))
                throw new InvalidOperationException($"Codeberg authorization failed: {error}");

            if (!string.Equals(state, expectedState, StringComparison.Ordinal))
                throw new InvalidOperationException("Codeberg authorization state mismatch.");

            if (string.IsNullOrWhiteSpace(code))
                throw new InvalidOperationException("Codeberg authorization callback did not contain a code.");

            return code;
        }
        finally
        {
            listener.Stop();
        }
    }

    /// <summary>
    /// Parses query string values.
    /// </summary>
    private static Dictionary<string, string> ParseQuery(string query)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(query))
            return values;

        var trimmed = query.StartsWith('?') ? query[1..] : query;
        foreach (var part in trimmed.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var index = part.IndexOf('=');
            var key = index >= 0 ? part[..index] : part;
            var value = index >= 0 ? part[(index + 1)..] : string.Empty;

            values[Uri.UnescapeDataString(key.Replace('+', ' '))] =
                Uri.UnescapeDataString(value.Replace('+', ' '));
        }

        return values;
    }

    /// <summary>
    /// Writes the browser callback response.
    /// </summary>
    private static async Task WriteResponseAsync(
        Stream stream,
        string message,
        CancellationToken ct)
    {
        var body = $"""
            <html>
              <body style="font-family: sans-serif; padding: 24px;">
                <h2>{System.Net.WebUtility.HtmlEncode(message)}</h2>
              </body>
            </html>
            """;

        var response =
            "HTTP/1.1 200 OK\r\n" +
            "Content-Type: text/html; charset=utf-8\r\n" +
            $"Content-Length: {Encoding.UTF8.GetByteCount(body)}\r\n" +
            "Connection: close\r\n\r\n" +
            body;

        var bytes = Encoding.UTF8.GetBytes(response);
        await stream.WriteAsync(bytes, ct);
        await stream.FlushAsync(ct);
    }

    /// <summary>
    /// Opens the authorization URL in a browser.
    /// </summary>
    private static void TryOpenBrowser(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url)
            {
                UseShellExecute = true
            });
        }
        catch
        {
            AnsiConsole.MarkupLine($"[yellow]Open this URL manually:[/] {url}");
        }
    }

    /// <summary>
    /// Creates a PKCE pair.
    /// </summary>
    private static PkcePair CreatePkcePair()
    {
        var verifier = CreateUrlSafeRandomString(64);
        var challenge = CreateCodeChallenge(verifier);
        return new PkcePair(verifier, challenge);
    }

    /// <summary>
    /// Creates the OAuth state value.
    /// </summary>
    private static string CreateState()
        => CreateUrlSafeRandomString(32);

    /// <summary>
    /// Creates a URL-safe random string.
    /// </summary>
    private static string CreateUrlSafeRandomString(int length)
    {
        Span<byte> buffer = stackalloc byte[length];
        RandomNumberGenerator.Fill(buffer);
        return ToBase64Url(buffer);
    }

    /// <summary>
    /// Creates the PKCE challenge.
    /// </summary>
    private static string CreateCodeChallenge(string verifier)
    {
        var hash = SHA256.HashData(Encoding.ASCII.GetBytes(verifier));
        return ToBase64Url(hash);
    }

    /// <summary>
    /// Encodes bytes as base64url.
    /// </summary>
    private static string ToBase64Url(ReadOnlySpan<byte> bytes)
        => Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    /// <summary>
    /// Exchanges the authorization code for an access token.
    /// </summary>
    private async Task<string> ExchangeCodeAsync(
        CodebergPreset preset,
        string clientId,
        string code,
        string codeVerifier,
        CancellationToken ct)
    {
        var clientSecret = ResolveClientSecret();
        var request = new HttpRequestMessage(HttpMethod.Post, preset.TokenEndpoint)
        {
            Content = new FormUrlEncodedContent(CreateTokenRequest(clientId, clientSecret, code, codeVerifier))
        };

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await Http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException(
                $"Codeberg token exchange failed ({(int)response.StatusCode}): {body}".Trim());
        }

        var payload = await response.Content.ReadFromJsonAsync<AccessTokenResponse>(cancellationToken: ct)
                      ?? throw new InvalidOperationException($"Invalid {preset.DisplayName} token response.");

        if (string.IsNullOrWhiteSpace(payload.AccessToken))
            throw new InvalidOperationException($"Invalid {preset.DisplayName} token response.");

        return payload.AccessToken;
    }

    /// <summary>
    /// Creates the token exchange payload.
    /// </summary>
    private static Dictionary<string, string> CreateTokenRequest(
        string clientId,
        string? clientSecret,
        string code,
        string codeVerifier)
    {
        var request = new Dictionary<string, string>
        {
            ["client_id"] = clientId,
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = RedirectUri.ToString(),
            ["code_verifier"] = codeVerifier
        };

        if (!string.IsNullOrWhiteSpace(clientSecret))
            request["client_secret"] = clientSecret;

        return request;
    }

    /// <summary>
    /// Validates an access token against the user endpoint.
    /// </summary>
    private async Task<bool> ValidateTokenAsync(
        CodebergPreset preset,
        string token,
        CancellationToken ct)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, preset.UserEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.UserAgent.ParseAdd("ChangeTrace");

        using var response = await Http.SendAsync(request, ct);
        return response.IsSuccessStatusCode;
    }
}
