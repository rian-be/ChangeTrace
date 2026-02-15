using ChangeTrace.Core.Results;

namespace ChangeTrace.Core.Models;

/// <summary>
/// Represents validated actor (user) name in system.
/// 
/// Enforces non-empty, trimmed strings with a maximum length of 200 characters.
/// Designed as  <see cref="ValueObject"/> for use in domain rules and specifications.
/// </summary>
internal sealed record ActorName : ValueObject
{
    private string Value { get; }
    private ActorName(string value) => Value = value;

    /// <summary>
    /// Attempts to create an <see cref="ActorName"/> from a string.
    /// Returns a failure if the string is null, whitespace, or too long.
    /// </summary>
    /// <param name="value">Candidate actor name</param>
    /// <returns>Result containing a validated <see cref="ActorName"/> or an error</returns>
    public static Result<ActorName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<ActorName>.Failure("Actor name cannot be empty");

        value = value.Trim();

        return value.Length > 200
            ? Result<ActorName>.Failure("Actor name too long")
            : Result<ActorName>.Success(new ActorName(value));
    }
    
    public override string ToString() => Value;
    public static implicit operator string(ActorName actor) => actor.Value;
}