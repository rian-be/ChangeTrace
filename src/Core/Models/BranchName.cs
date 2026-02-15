using ChangeTrace.Core.Results;

namespace ChangeTrace.Core.Models;

/// <summary>
/// Represents validated Git branch name.
/// 
/// Enforces non-empty strings, maximum length of 255, no reserved names,
/// and basic format rules (no leading/trailing dot, no consecutive dots).
/// Supports detection of remote branches and extraction of local branch name.
/// </summary>
internal sealed record BranchName : ValueObject
{
    /// <summary>
    /// Reserved branch names that are not allowed.
    /// </summary>
    private static readonly HashSet<string> ReservedNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "HEAD", "-", "@"
    };

    public string Value { get; }
    public bool IsRemote => Value.Contains('/');
    public string LocalName => IsRemote ? Value.Split('/').Last() : Value;

    private BranchName(string value) => Value = value;

    /// <summary>
    /// Attempts to create a <see cref="BranchName"/> from a string.
    /// Returns a failure if the name is empty, too long, reserved, or malformed.
    /// </summary>
    /// <param name="value">Candidate branch name</param>
    /// <returns>Result containing a validated <see cref="BranchName"/> or an error</returns>
    public static Result<BranchName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<BranchName>.Failure("Branch name cannot be empty");

        value = value.Trim();

        if (value.Length > 255)
            return Result<BranchName>.Failure("Branch name too long");

        if (ReservedNames.Contains(value))
            return Result<BranchName>.Failure($"'{value}' is reserved");

        if (value.Contains("..") || value.StartsWith('.') || value.EndsWith('.'))
            return Result<BranchName>.Failure("Invalid branch name format");

        return Result<BranchName>.Success(new BranchName(value));
    }

    public override string ToString() => Value;
    public static implicit operator string(BranchName branch) => branch.Value;
}