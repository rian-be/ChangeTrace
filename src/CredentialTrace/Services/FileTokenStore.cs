using System.Diagnostics;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ChangeTrace.Configuration.Discovery;
using ChangeTrace.CredentialTrace.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.CredentialTrace.Services;

/// <summary>
/// Stores authentication sessions in a local encrypted JSON file.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class FileTokenStore : ITokenStore
{
    private const int StoreVersion = 1;
    private const int KeySizeBytes = 32;
    private const int NonceSizeBytes = 12;
    private const int TagSizeBytes = 16;
    private const string Algorithm = "AES-GCM";

    private static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true
    };

    private readonly SemaphoreSlim _gate = new(1, 1);

    private readonly string _directory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".changetrace");

    private readonly string _path;
    private readonly string _keyPath;

    public FileTokenStore()
    {
        _path = Path.Combine(_directory, "auth.json");
        _keyPath = Path.Combine(_directory, "auth.key");
    }

    public async Task SaveAsync(AuthSession session, CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct);
        try
        {
            var all = await ReadAllUnsafeAsync(ct);
            all[session.Provider] = session;

            await WriteAllUnsafeAsync(all.Values, ct);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<AuthSession?> GetAsync(string provider, CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct);
        try
        {
            var all = await ReadAllUnsafeAsync(ct);
            return all.TryGetValue(provider, out var session)
                ? session
                : null;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<IReadOnlyList<AuthSession>> ListAsync(CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct);
        try
        {
            return [.. (await ReadAllUnsafeAsync(ct)).Values];
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task RemoveAsync(string provider, CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct);
        try
        {
            var all = await ReadAllUnsafeAsync(ct);
            all.Remove(provider);

            await WriteAllUnsafeAsync(all.Values, ct);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<Dictionary<string, AuthSession>> ReadAllUnsafeAsync(CancellationToken ct)
    {
        if (!File.Exists(_path))
            return [];

        var json = await File.ReadAllTextAsync(_path, ct);

        if (string.IsNullOrWhiteSpace(json))
            return [];

        try
        {
            using var document = JsonDocument.Parse(json);

            if (document.RootElement.ValueKind == JsonValueKind.Array)
                return ReadLegacySessions(json);

            var store = JsonSerializer.Deserialize<TokenStoreDocument>(json, Json);

            if (store?.Version != StoreVersion)
                return [];

            var sessions = new Dictionary<string, AuthSession>(StringComparer.OrdinalIgnoreCase);

            foreach (var stored in store.Sessions)
            {
                var accessToken = DecryptToken(stored.AccessToken);
                var session = stored.ToSession(accessToken);

                sessions[session.Provider] = session;
            }

            return sessions;
        }
        catch (JsonException)
        {
            return [];
        }
        catch (CryptographicException)
        {
            return [];
        }
        catch (FormatException)
        {
            return [];
        }
    }

    private static Dictionary<string, AuthSession> ReadLegacySessions(string json)
    {
        var legacy = JsonSerializer.Deserialize<List<AuthSession>>(json, Json) ?? [];

        return legacy
            .Where(x => !string.IsNullOrWhiteSpace(x.Provider))
            .ToDictionary(x => x.Provider, StringComparer.OrdinalIgnoreCase);
    }

    private async Task WriteAllUnsafeAsync(IEnumerable<AuthSession> sessions, CancellationToken ct)
    {
        EnsureStoreDirectory();

        var store = new TokenStoreDocument(
            StoreVersion,
            sessions
                .Where(x => !string.IsNullOrWhiteSpace(x.Provider))
                .Select(x => StoredAuthSession.FromSession(
                    x,
                    EncryptToken(x.AccessToken)))
                .ToArray());

        var json = JsonSerializer.Serialize(store, Json);

        var tempPath = $"{_path}.{Guid.NewGuid():N}.tmp";

        await File.WriteAllTextAsync(tempPath, json, ct);
        RestrictFileAccess(tempPath);

        File.Move(tempPath, _path, overwrite: true);
        RestrictFileAccess(_path);
    }

    private TokenEnvelope EncryptToken(string accessToken)
    {
        var key = GetOrCreateKey();
        var nonce = RandomNumberGenerator.GetBytes(NonceSizeBytes);
        var plaintext = Encoding.UTF8.GetBytes(accessToken);
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[TagSizeBytes];

        using var aes = new AesGcm(key, TagSizeBytes);
        aes.Encrypt(nonce, plaintext, ciphertext, tag);

        CryptographicOperations.ZeroMemory(plaintext);

        return new TokenEnvelope(
            Algorithm,
            Convert.ToBase64String(nonce),
            Convert.ToBase64String(ciphertext),
            Convert.ToBase64String(tag));
    }

    private string DecryptToken(TokenEnvelope envelope)
    {
        if (!string.Equals(envelope.Algorithm, Algorithm, StringComparison.Ordinal))
            throw new InvalidOperationException($"Unsupported token encryption algorithm: {envelope.Algorithm}");

        var key = GetOrCreateKey();
        var nonce = Convert.FromBase64String(envelope.Nonce);
        var ciphertext = Convert.FromBase64String(envelope.CipherText);
        var tag = Convert.FromBase64String(envelope.Tag);
        var plaintext = new byte[ciphertext.Length];

        using var aes = new AesGcm(key, TagSizeBytes);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);

        var accessToken = Encoding.UTF8.GetString(plaintext);
        CryptographicOperations.ZeroMemory(plaintext);

        return accessToken;
    }

    private byte[] GetOrCreateKey()
    {
        EnsureStoreDirectory();

        if (File.Exists(_keyPath))
        {
            var existing = Convert.FromBase64String(File.ReadAllText(_keyPath));

            if (existing.Length == KeySizeBytes)
                return existing;
        }

        var key = RandomNumberGenerator.GetBytes(KeySizeBytes);

        File.WriteAllText(_keyPath, Convert.ToBase64String(key));
        RestrictFileAccess(_keyPath);

        return key;
    }

    private void EnsureStoreDirectory()
    {
        Directory.CreateDirectory(_directory);
        RestrictFileAccess(_directory, isDirectory: true);
    }

    private static void RestrictFileAccess(string path, bool isDirectory = false)
    {
        if (OperatingSystem.IsWindows())
        {
            TrySetWindowsAcl(path, isDirectory);
            return;
        }

        var mode = isDirectory
            ? UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute
            : UnixFileMode.UserRead | UnixFileMode.UserWrite;

        TrySetUnixMode(path, mode);
    }

    [SupportedOSPlatform("windows")]
    private static void TrySetWindowsAcl(string path, bool isDirectory)
    {
        try
        {
            var identity = $"{Environment.UserDomainName}\\{Environment.UserName}";
            var rights = isDirectory ? "(OI)(CI)(F)" : "(F)";

            var startInfo = new ProcessStartInfo
            {
                FileName = "icacls",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            startInfo.ArgumentList.Add(path);
            startInfo.ArgumentList.Add("/inheritance:r");
            startInfo.ArgumentList.Add("/grant:r");
            startInfo.ArgumentList.Add($"{identity}:{rights}");

            using var process = Process.Start(startInfo);
            process?.WaitForExit();
        }
        catch
        {
            // Best-effort hardening only.
        }
    }

    [UnsupportedOSPlatform("windows")]
    private static void TrySetUnixMode(string path, UnixFileMode mode)
    {
        try
        {
            File.SetUnixFileMode(path, mode);
        }
        catch
        {
            // Best-effort hardening only.
        }
    }

    private sealed record TokenStoreDocument(
        int Version,
        IReadOnlyList<StoredAuthSession> Sessions);

    private sealed record StoredAuthSession(
        string Provider,
        TokenEnvelope AccessToken,
        string? Username,
        Ulid Id,
        DateTimeOffset CreatedAt)
    {
        public static StoredAuthSession FromSession(AuthSession session, TokenEnvelope accessToken) =>
            new(session.Provider, accessToken, session.Username, session.Id, session.CreatedAt);

        public AuthSession ToSession(string accessToken) =>
            new(Provider, accessToken, Username, Id, CreatedAt);
    }

    private sealed record TokenEnvelope(
        string Algorithm,
        string Nonce,
        string CipherText,
        string Tag);
}