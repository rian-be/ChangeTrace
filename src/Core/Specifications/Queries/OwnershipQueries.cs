using ChangeTrace.Core.Events;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Specifications.Filters;
using ChangeTrace.Core.Specifications.Filters.Ownership;

namespace ChangeTrace.Core.Specifications.Queries;

/// <summary>
/// Ownership queries providing reusable specifications for <see cref="TraceEvent"/> instances.
/// These queries allow filtering events based on file or directory ownership and maintainership.
/// </summary>
/// <remarks>
/// Use this class to compose specifications for filtering events by ownership criteria:
/// - Contributors to a specific directory
/// - Contributors to a specific file
/// - Sole maintainers of a file based on minimum commits
/// </remarks>
internal static class OwnershipQueries
{
    /// <summary>
    /// Creates specification for contributors to a specific directory.
    /// </summary>
    /// <param name="directory">The directory for which ownership is evaluated.</param>
    /// <returns>
    /// A specification matching commits made in the given <paramref name="directory"/>.
    /// Combines <see cref="InDirectorySpec"/> with <see cref="CommitsOnlySpec"/>.
    /// </returns>
    public static Specification<TraceEvent> DirectoryOwners(string directory)
        => new InDirectorySpec(directory).And(new CommitsOnlySpec());

    /// <summary>
    /// Creates specification for contributors to a specific file.
    /// </summary>
    /// <param name="path">The file path for which ownership is evaluated.</param>
    /// <returns>
    /// A specification matching commits made to the given <paramref name="path"/>.
    /// Combines <see cref="ByFileSpec"/> with <see cref="CommitsOnlySpec"/>.
    /// </returns>
    public static Specification<TraceEvent> FileOwners(FilePath path)
        => new ByFileSpec(path).And(new CommitsOnlySpec());

    /// <summary>
    /// Creates specification for identifying sole maintainers of a file.
    /// </summary>
    /// <param name="allEvents">All <see cref="TraceEvent"/> instances in the repository.</param>
    /// <param name="path">The file path to evaluate maintainership for.</param>
    /// <param name="minCommits">The minimum number of commits required to be considered a sole maintainer.</param>
    /// <returns>
    /// A specification matching the author(s) who dominate commits on the given <paramref name="path"/>
    /// based on the <paramref name="minCommits"/> threshold.
    /// </returns>
    public static Specification<TraceEvent> SoleMaintainers(
        IEnumerable<TraceEvent> allEvents, FilePath path, int minCommits)
        => new AuthorDominatesFileSpec(allEvents, path, minCommits);
}