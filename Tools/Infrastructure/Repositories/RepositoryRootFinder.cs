namespace ChangeTrace.Tools.Infrastructure.Repositories;

/// <summary>
/// Locates the repository root directory.
/// </summary>
public sealed class RepositoryRootFinder : IRepositoryRootFinder
{
    /// <summary>
    /// Finds the repository root containing the ChangeTrace project file.
    /// </summary>
    public string Find()
    {
        DirectoryInfo? directory =
            new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            string projectPath =
                Path.Combine(
                    directory.FullName,
                    "ChangeTrace.csproj");

            if (File.Exists(projectPath))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not find repository root containing ChangeTrace.csproj.");
    }
}