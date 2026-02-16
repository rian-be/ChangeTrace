namespace ChangeTrace.GIt.Interfaces;

/// <summary>
/// Abstraction over file system operations for timeline storage.
/// Provides methods to save/load files, ensure extensions, and enumerate files.
/// </summary>
internal interface IFileManager
{
    /// <summary>
    /// Saves byte array data to the specified file path.
    /// Ensures parent directories exist.
    /// </summary>
    /// <param name="path">Full file path where data should be saved.</param>
    /// <param name="data">Byte array to write to disk.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the async operation.</returns>
    Task SaveAsync(string path, byte[] data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads byte array data from the specified file path.
    /// </summary>
    /// <param name="path">Full file path to read from.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Byte array content of the file.</returns>
    Task<byte[]> LoadAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ensures the provided path has the specified file extension.
    /// Adds the extension if missing.
    /// </summary>
    /// <param name="path">Input file path.</param>
    /// <param name="extension">Extension to ensure (including leading dot, e.g., ".gittrace").</param>
    /// <returns>File path guaranteed to have the specified extension.</returns>
    string EnsureExtension(string path, string extension);

    /// <summary>
    /// Enumerates all files in directory matching the given extension.
    /// </summary>
    /// <param name="directory">Directory to search in.</param>
    /// <param name="extension">File extension to filter by (including leading dot).</param>
    /// <returns>Enumerable of file paths matching the extension.</returns>
    IEnumerable<string> FindFiles(string directory, string extension);
}