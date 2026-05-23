namespace ChangeTrace.Tools.Infrastructure.Repositories;

/// <summary>
/// Resolves the repository root directory.
/// </summary>
public interface IRepositoryRootFinder
{
    /// <summary>
    /// Finds the repository root path.
    /// </summary>
    string Find();
}