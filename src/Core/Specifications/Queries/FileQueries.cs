using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Specifications.Filters;

namespace ChangeTrace.Core.Specifications.Queries;

/// <summary>
/// File queries providing reusable specifications for <see cref="TraceEvent"/> instances.
/// These queries allow filtering events based on file type, modifications, and author activity.
/// </summary>
/// <remarks>
/// Use this class to compose specifications for filtering events by file-related criteria:
/// - Changes to files with specific extensions
/// - Modifications made by a specific actor in a specific directory
/// </remarks>
internal static class FileQueries
{
    /// <summary>
    /// Creates specification for changes to Python files.
    /// </summary>
    /// <returns>A specification matching commits that modify files with the ".py" extension.</returns>
    internal static Specification<TraceEvent> PythonChanges()
        => new ByExtensionSpec(".py");

    /// <summary>
    /// Creates specification for file modifications made by a specific actor in a specific directory.
    /// </summary>
    /// <param name="actor">The actor who made the modifications.</param>
    /// <param name="directory">The directory in which the modifications occurred.</param>
    /// <returns>
    /// A specification matching events that are file modifications (<see cref="CommitEventType.FileModified"/>)
    /// made by the given <paramref name="actor"/> in the specified <paramref name="directory"/>.
    /// Combines <see cref="ByCommitTypeSpec"/>, <see cref="ByActorSpec"/>, and <see cref="InDirectorySpec"/>.
    /// </returns>
    internal static Specification<TraceEvent> ModificationsByAuthor(
        ActorName actor,
        string directory)
    {
        return new ByCommitTypeSpec(CommitEventType.FileModified)
            .And(new ByActorSpec(actor))
            .And(new InDirectorySpec(directory));
    }
}