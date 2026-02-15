using ChangeTrace.Core.Results;

namespace ChangeTrace.Core.Models;

/// <summary>
/// Represents validated pull request number.
/// 
/// Ensures the number is positive and provides convenient string and integer conversions.
/// </summary>
internal readonly struct PullRequestNumber
{
    private int Value { get; }
    private PullRequestNumber(int value) => Value = value;

    /// <summary>
    /// Attempts to create a <see cref="PullRequestNumber"/>.
    /// Returns failure if the number is zero or negative.
    /// </summary>
    /// <param name="value">Candidate PR number</param>
    /// <returns>Result containing a validated <see cref="PullRequestNumber"/> or an error</returns>
    public static Result<PullRequestNumber> Create(int value)  =>
         value <= 0? Result<PullRequestNumber>.Failure("PR number must be positive") 
             :  Result<PullRequestNumber>.Success(new PullRequestNumber(value));

    public override string ToString() => $"#{Value}";
    public static implicit operator int(PullRequestNumber pr) => pr.Value;
}