using ChangeTrace.Configuration;
using ChangeTrace.Core;
using ChangeTrace.GIt.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.GIt.Services;

/// <summary>
/// Simple file manager for timelines.
/// Handles reading, writing, and file discovery with extension management.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class FileManager : IFileManager
{
    /// <summary>
    /// Saves data to the specified file path.
    /// Automatically creates the directory if it does not exist.
    /// </summary>
    /// <param name="path">Target file path.</param>
    /// <param name="data">Byte array to save.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task SaveAsync(string path, byte[] data, CancellationToken cancellationToken = default)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        return File.WriteAllBytesAsync(path, data, cancellationToken);
    }

    /// <summary>
    /// Loads data from the specified file path.
    /// </summary>
    /// <param name="path">File path to read.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Byte array with file contents.</returns>
    public Task<byte[]> LoadAsync(string path, CancellationToken cancellationToken = default)
        => File.ReadAllBytesAsync(path, cancellationToken);

    /// <summary>
    /// Ensures that the given path ends with the specified extension.
    /// If not, appends the extension.
    /// </summary>
    /// <param name="path">File path to check.</param>
    /// <param name="extension">Extension to enforce (including dot, e.g., ".gittrace").</param>
    /// <returns>File path with guaranteed extension.</returns>
    public string EnsureExtension(string path, string extension)
        => path.EndsWith(extension) ? path : path + extension;

    /// <summary>
    /// Enumerates all files in the given directory matching the specified extension.
    /// Returns empty collection if directory does not exist.
    /// </summary>
    /// <param name="directory">Directory to search.</param>
    /// <param name="extension">File extension to filter by (including dot, e.g., ".gittrace").</param>
    /// <returns>Enumerable of matching file paths.</returns>
    public IEnumerable<string> FindFiles(string directory, string extension)
        => Directory.Exists(directory) 
            ? Directory.EnumerateFiles(directory, "*" + extension) 
            : [];
}