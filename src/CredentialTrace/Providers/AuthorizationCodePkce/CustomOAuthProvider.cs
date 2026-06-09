using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ChangeTrace.Configuration.Discovery;
using ChangeTrace.CredentialTrace.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace ChangeTrace.CredentialTrace.Providers.AuthorizationCodePkce;

/// <summary>
/// OAuth provider for user-defined OIDC servers.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class CustomOAuthProvider(HttpClient http) : IAuthProvider
{
    private HttpClient Http { get; } = http;

    private const int RedirectPort = 8765;
    private const string RedirectPath = "/callback";
    private const string Scope = "openid profile email";
    private static readonly Uri RedirectUri = new($"http://127.0.0.1:{RedirectPort}{RedirectPath}");

    /// <summary>
    /// PKCE verifier/challenge pair.
    /// </summary>
    private readonly record struct PkcePair(string Verifier, string Challenge);

    /// <summary>
    /// OIDC discovery document.
    /// </summary>
    private sealed record DiscoveryDocument(
        [property: JsonPropertyName("issuer")] string? Issuer,
        [property: JsonPropertyName("authorization_endpoint")] string AuthorizationEndpoint,
        [property: JsonPropertyName("token_endpoint")] string TokenEndpoint,
        [property: JsonPropertyName("userinfo_endpoint")] string? UserInfoEndpoint);

    /// <summary>
    /// OIDC access token response.
    /// </summary>
    private sealed record AccessTokenResponse(
        [property: JsonPropertyName("access_token")] string? AccessToken);

    /// <summary>
    /// Gets the unique provider name.
    /// </summary>
    public string Name => "custom";

    /// <summary>
    /// Gets whether the provider is configured.
    /// </summary>
    public bool IsConfigured => true;

    /// <summary>
    /// Performs an OIDC login against a user-defined provider.
    /// </summary>
    public async Task<AuthSession> LoginAsync(CancellationToken ct = default)
    {
        var providerName = PromptProviderName();
        var issuer = PromptIssuer();
        var clientId = PromptClientId(issuer);
        var discovery = await FetchDiscoveryAsync(issuer, ct);
        var pkce = CreatePkcePair();
        var state = CreateState();
        var authorizationUrl = BuildAuthorizationUrl(discovery.AuthorizationEndpoint, clientId, pkce.Challenge, state);

        ShowLoginInstructions(providerName, issuer, authorizationUrl);

        using var listener = CreateListener();
        var codeTask = WaitForAuthorizationCodeAsync(listener, state, ct);

        TryOpenBrowser(authorizationUrl);

        var code = await codeTask;
        var accessToken = await ExchangeCodeAsync(discovery.TokenEndpoint, clientId, code, pkce.Verifier, ct);
        var username = await TryFetchUsernameAsync(discovery.UserInfoEndpoint, accessToken, ct);

        return AuthSession.Create(BuildSessionProviderKey(providerName), accessToken, username);
    }

    /// <summary>
    /// Builds the stored session provider key.
    /// </summary>
    private static string BuildSessionProviderKey(string providerName)
    {
        var slug = NormalizeSlug(providerName);
        return $"custom:{slug}";
    }

    /// <summary>
    /// Prompts for the provider name.
    /// </summary>
    private static string PromptProviderName()
    {
        var providerName = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter provider name")
                .PromptStyle("blue")
                .Validate(value =>
                    !string.IsNullOrWhiteSpace(value)
                        ? ValidationResult.Success()
                        : ValidationResult.Error("[red]Provider name is required.[/]")));

        return NormalizeSlug(providerName);
    }

    /// <summary>
    /// Prompts for the issuer URL.
    /// </summary>
    private static string PromptIssuer()
    {
        var issuer = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter OIDC issuer URL")
                .PromptStyle("blue")
                .Validate(value =>
                    Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
                    (uri.Scheme is "https" || uri.Host is "localhost" || uri.Host is "127.0.0.1")
                        ? ValidationResult.Success()
                        : ValidationResult.Error("[red]Enter a valid https URL or localhost issuer.[/]")));

        return issuer.Trim().TrimEnd('/');
    }

    /// <summary>
    /// Prompts for the client id.
    /// </summary>
    private static string PromptClientId(string issuer)
    {
        var host = new Uri(issuer).Host;
        return AnsiConsole.Prompt(
            new TextPrompt<string>($"Enter client id for [bold]{host}[/]")
                .PromptStyle("blue")
                .Validate(value => !string.IsNullOrWhiteSpace(value)
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Client id is required.[/]")))
            .Trim();
    }

    /// <summary>
    /// Fetches the OIDC discovery document.
    /// </summary>
    private async Task<DiscoveryDocument> FetchDiscoveryAsync(string issuer, CancellationToken ct)
    {
        var discoveryUri = $"{issuer}/.well-known/openid-configuration";
        var doc = await Http.GetFromJsonAsync<DiscoveryDocument>(discoveryUri, ct)
                  ?? throw new InvalidOperationException("OIDC discovery document is invalid.");

        if (string.IsNullOrWhiteSpace(doc.AuthorizationEndpoint) || string.IsNullOrWhiteSpace(doc.TokenEndpoint))
            throw new InvalidOperationException("OIDC discovery document is missing endpoints.");

        return doc;
    }

    /// <summary>
    /// Builds the authorization URL.
    /// </summary>
    private static string BuildAuthorizationUrl(
        string authorizationEndpoint,
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

        return $"{authorizationEndpoint}?{string.Join("&", query.Select(pair => $"{pair.Key}={Uri.EscapeDataString(pair.Value)}"))}";
    }

    /// <summary>
    /// Shows login instructions.
    /// </summary>
    private static void ShowLoginInstructions(string providerName, string issuer, string authorizationUrl)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(
            new Panel(
                    $"[bold]Custom OIDC login[/]\n" +
                    $"[grey]Provider:[/] [white]{providerName}[/]\n" +
                    $"[grey]Issuer:[/] [white]{issuer}[/]\n" +
                    $"[grey]Redirect URI:[/] [white]{RedirectUri}[/]\n" +
                    $"[grey]Authorization URL:[/] [blue underline]{authorizationUrl}[/]")
                .Border(BoxBorder.Double)
                .Padding(1, 1)
                .Header("[bold]Custom OAuth[/]"));
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

            var requestLine = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(requestLine))
                throw new InvalidOperationException("Custom authorization callback was empty.");

            while (!string.IsNullOrEmpty(await reader.ReadLineAsync()))
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
                throw new InvalidOperationException($"Custom authorization failed: {error}");

            if (!string.Equals(state, expectedState, StringComparison.Ordinal))
                throw new InvalidOperationException("Custom authorization state mismatch.");

            if (string.IsNullOrWhiteSpace(code))
                throw new InvalidOperationException("Custom authorization callback did not contain a code.");

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
    /// Normalizes a provider slug.
    /// </summary>
    private static string NormalizeSlug(string value)
    {
        var slug = value.Trim().ToLowerInvariant();
        var builder = new StringBuilder(slug.Length);

        foreach (var ch in slug)
        {
            if (char.IsLetterOrDigit(ch))
                builder.Append(ch);
            else if (ch is '-' or '_' or '.')
                builder.Append('-');
        }

        return builder.Length > 0 ? builder.ToString().Trim('-') : "custom";
    }

    /// <summary>
    /// Exchanges the authorization code for an access token.
    /// </summary>
    private async Task<string> ExchangeCodeAsync(
        string tokenEndpoint,
        string clientId,
        string code,
        string codeVerifier,
        CancellationToken ct)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = clientId,
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["redirect_uri"] = RedirectUri.ToString(),
                ["code_verifier"] = codeVerifier
            })
        };

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await Http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException(
                $"Custom token exchange failed ({(int)response.StatusCode}): {body}".Trim());
        }

        var payload = await response.Content.ReadFromJsonAsync<AccessTokenResponse>(cancellationToken: ct)
                      ?? throw new InvalidOperationException("Invalid custom token response.");

        if (string.IsNullOrWhiteSpace(payload.AccessToken))
            throw new InvalidOperationException("Invalid custom token response.");

        return payload.AccessToken;
    }

    /// <summary>
    /// Tries to fetch the username from the OIDC userinfo endpoint.
    /// </summary>
    private async Task<string?> TryFetchUsernameAsync(
        string? userInfoEndpoint,
        string token,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userInfoEndpoint))
            return null;

        var request = new HttpRequestMessage(HttpMethod.Get, userInfoEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await Http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
            return null;

        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        if (document.RootElement.TryGetProperty("preferred_username", out var preferred) && preferred.ValueKind == JsonValueKind.String)
            return preferred.GetString();

        if (document.RootElement.TryGetProperty("username", out var username) && username.ValueKind == JsonValueKind.String)
            return username.GetString();

        if (document.RootElement.TryGetProperty("email", out var email) && email.ValueKind == JsonValueKind.String)
            return email.GetString();

        return null;
    }
}
