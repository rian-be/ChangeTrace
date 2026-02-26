namespace ChangeTrace.GIt.Helpers;

/// <summary>
/// Helper responsible for detecting repository hosting provider
/// based on repository URL or SSH string.
/// </summary>
/// <remarks>
/// Supports HTTPS and SSH formats.
/// Currently maps known hosts to provider identifiers.
/// Throws explicit exceptions for unsupported or invalid inputs.
/// </remarks>
internal static class ProviderUrlHelper
{
    /// <summary>
    /// Mapping between repository host names and internal provider identifiers.
    /// </summary>
    private static readonly Dictionary<string, string> HostProviderMap =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["github.com"] = "github"
        };

    /// <summary>
    /// Detects repository provider from repository URL or SSH string.
    /// </summary>
    /// <param name="repository">
    /// Repository location in HTTPS (e.g., https://github.com/org/repo.git)
    /// or SSH format (e.g., git@github.com:org/repo.git).
    /// </param>
    /// <returns>
    /// Provider identifier (e.g., "github").
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when repository string is null, empty, or whitespace.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when host cannot be extracted from the repository string.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Thrown when repository host is not mapped to a supported provider.
    /// </exception>
    internal static string DetectProvider(string repository)
    {
        if (string.IsNullOrWhiteSpace(repository))
            throw new ArgumentException("Repository cannot be null or empty.", nameof(repository));

        var host = ExtractHost(repository);

        if (host is null)
            throw new InvalidOperationException("Unable to determine repository host.");

        return HostProviderMap.TryGetValue(host, out var provider)
            ? provider
            : throw new NotSupportedException($"Repository host '{host}' is not supported.");
    }

    /// <summary>
    /// Extracts host name from repository string.
    /// Supports HTTPS/HTTP and SSH Git formats.
    /// </summary>
    /// <param name="repository">Repository URL or SSH string.</param>
    /// <returns>
    /// Host name if successfully parsed; otherwise null.
    /// </returns>
    private static string? ExtractHost(string repository)
    {
        // HTTPS / HTTP
        if (Uri.TryCreate(repository, UriKind.Absolute, out var uri))
            return uri.Host;

        // SSH format: git@github.com:org/repo.git
        var atIndex = repository.IndexOf('@');
        var colonIndex = repository.IndexOf(':');

        if (atIndex < 0 || colonIndex <= atIndex)
            return null;

        var hostPart = repository.Substring(atIndex + 1, colonIndex - atIndex - 1);
        return hostPart;
    }
}