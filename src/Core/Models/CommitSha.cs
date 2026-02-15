using System.Text.RegularExpressions;
using ChangeTrace.Core.Results;

namespace ChangeTrace.Core.Models;

/// <summary>
/// Represents validated Git commit SHA with optional partial matching.
/// 
/// Ensures non-empty, lowercase hexadecimal strings between 7 and 40 characters.
/// Supports short form display and partial matching for comparisons.
/// </summary>
internal sealed partial record CommitSha : ValueObject
{
    /// <summary>
    /// Regex for validating SHA format (7–40 hex characters).
    /// </summary>
    private static readonly Regex ShaRegex = MyRegex();

    public string Value { get; }
    private string Short => Value.Length > 7 ? Value[..7] : Value;

    private CommitSha(string value) => Value = value;
    
    /// <summary>
    /// Attempts to create a <see cref="CommitSha"/> from a string.
    /// Returns a failure if empty, too short/long, or not hexadecimal.
    /// </summary>
    /// <param name="value">Candidate SHA string</param>
    /// <returns>Result containing a validated <see cref="CommitSha"/> or an error</returns>
    public static Result<CommitSha> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<CommitSha>.Failure("SHA cannot be empty");

        value = value.Trim().ToLowerInvariant();

        if (value.Length is < 7 or > 40)
            return Result<CommitSha>.Failure("SHA must be 7-40 characters");

        return !ShaRegex.IsMatch(value)
            ? Result<CommitSha>.Failure("SHA must be hexadecimal")
            : Result<CommitSha>.Success(new CommitSha(value));
    }

    /// <summary>
    /// Checks whether this SHA matches another SHA, supporting partial matching.
    /// Compares up to the length of the shorter SHA.
    /// </summary>
    /// <param name="other">Other commit SHA to compare with</param>
    /// <returns>True if SHAs match for the overlapping length; otherwise false</returns>
    public bool Matches(CommitSha other)
    {
        var minLength = Math.Min(Value.Length, other.Value.Length);
        return Value[..minLength] == other.Value[..minLength];
    }

    public override string ToString() => Short;
    public static implicit operator string(CommitSha sha) => sha.Value;

    [GeneratedRegex("^[a-f0-9]{7,40}$", RegexOptions.Compiled)]
    private static partial Regex MyRegex();
}