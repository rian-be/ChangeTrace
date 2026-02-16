using ChangeTrace.Core.Results;

namespace ChangeTrace.Core.Models;

/// <summary>
/// Represents Git repository identifier in the format "owner/name".
/// 
/// Encapsulates owner and repository name with validation and provides
/// convenience methods for parsing and display.
/// </summary>
internal sealed record RepositoryId : ValueObject
{
    internal string Owner { get; }
    internal string Name { get; }

    internal string FullName => $"{Owner}/{Name}";

    private RepositoryId(string owner, string name)
    {
        Owner = owner;
        Name = name;
    }
    /// <summary>
    /// Creates <see cref="RepositoryId"/> from owner and name.
    /// Returns failure if either is empty or whitespace.
    /// </summary>
    /// <param name="owner">Repository owner</param>
    /// <param name="name">Repository name</param>
    /// <returns>Result containing a validated <see cref="RepositoryId"/> or an error</returns>
    public static Result<RepositoryId> Create(string owner, string name)
    {
        if (string.IsNullOrWhiteSpace(owner))
            return Result<RepositoryId>.Failure("Owner cannot be empty");

        return string.IsNullOrWhiteSpace(name)
            ? Result<RepositoryId>.Failure("Name cannot be empty") 
            : Result<RepositoryId>.Success(new RepositoryId(owner.Trim(), name.Trim()));
    }

    /// <summary>
    /// Parses full repository name in the format "owner/name".
    /// Returns failure if format is invalid or input is empty.
    /// </summary>
    /// <param name="fullName">Full repository name</param>
    /// <returns>Result containing a validated <see cref="RepositoryId"/> or an error</returns>
    public static Result<RepositoryId> Parse(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return Result<RepositoryId>.Failure("Full name cannot be empty");

        var parts = fullName.Split('/');
        if (parts.Length != 2)
            return Result<RepositoryId>.Failure("Format must be 'owner/name'");

        return Create(parts[0], parts[1]);
    }

    public override string ToString() => FullName;
}