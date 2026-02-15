namespace ChangeTrace.Core.Specifications;

/// <summary>
/// Base abstraction for Specification pattern.
/// 
/// Represents a composable business rule that can be evaluated
/// against a single instance of <typeparamref name="T"/>.
/// 
/// Specifications are immutable and side effect free —
/// evaluation must be deterministic and idempotent.
/// </summary>
/// <typeparam name="T">Domain type the rule applies to</typeparam>
internal abstract class Specification<T>
{
    /// <summary>
    /// Evaluates whether the specified item satisfies this specification.
    /// </summary>
    /// <returns>
    /// <c>true</c> when the item matches the rule;
    /// otherwise <c>false</c>.
    /// </returns>
    public abstract bool IsSatisfiedBy(T item);

    /// <summary>
    /// Combines this specification with another using logical AND.
    /// 
    /// Resulting specification is satisfied only when
    /// both specifications return <c>true</c>.
    /// </summary>
    /// <param name="other">Specification to combine with</param>
    /// <returns>Composite specification representing conjunction</returns>
    public Specification<T> And(Specification<T> other) 
        => new AndSpecification<T>(this, other);

    /// <summary>
    /// Combines this specification with another using logical OR.
    /// 
    /// Resulting specification is satisfied when
    /// at least one specification returns <c>true</c>.
    /// </summary>
    /// <param name="other">Specification to combine with</param>
    /// <returns>Composite specification representing disjunction</returns>
    public Specification<T> Or(Specification<T> other) 
        => new OrSpecification<T>(this, other);

    /// <summary>
    /// Negates this specification.
    /// 
    /// Resulting specification inverts the evaluation result.
    /// </summary>
    /// <returns>Specification representing logical negation</returns>
    public Specification<T> Not() 
        => new NotSpecification<T>(this);
}
