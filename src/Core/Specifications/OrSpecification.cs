namespace ChangeTrace.Core.Specifications;

/// <summary>
/// Logical disjunction of two <see cref="Specification{T}"/> instances.
/// 
/// The resulting specification is satisfied when at least one
/// of the composed specifications is satisfied.
/// 
/// Evaluation uses short-circuit semantics right specification
/// is evaluated only if the left specification returns <c>false</c>.
/// Both specifications must remain side effect free.
/// </summary>
/// <typeparam name="T">Domain type the rule applies to</typeparam>
/// <param name="left">Left side of the disjunction</param>
/// <param name="right">Right side of the disjunction</param>
internal sealed class OrSpecification<T>(Specification<T> left, Specification<T> right) : Specification<T>
{
    /// <summary>
    /// Evaluates the disjunction of the two specifications.
    /// </summary>
    /// <param name="item">Instance to test</param>
    /// <returns>
    /// <c>true</c> if either specification returns <c>true</c>;
    /// otherwise <c>false</c>.
    /// </returns>
    public override bool IsSatisfiedBy(T item) 
        => left.IsSatisfiedBy(item) || right.IsSatisfiedBy(item);
}