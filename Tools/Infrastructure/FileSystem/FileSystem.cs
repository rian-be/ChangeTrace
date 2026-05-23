namespace ChangeTrace.Tools.Infrastructure.FileSystem;

/// <summary>
/// Physical file system utilities.
/// </summary>
public sealed class FileSystem : IFileSystem
{
    /// <summary>
    /// Ensures that the parent directory for the given path exists.
    /// </summary>
    public void EnsureParentDirectory(string path)
    {
        string? directory = Path.GetDirectoryName(path);

        if (string.IsNullOrWhiteSpace(directory))
            return;

        Directory.CreateDirectory(directory);
    }
}