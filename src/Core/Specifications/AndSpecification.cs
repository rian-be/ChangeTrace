namespace ChangeTrace.Core.Specifications;

/// <summary>
/// Logical conjunction of two <see cref="Specification{T}"/> instances.
/// 
/// The resulting specification is satisfied only when both
/// composed specifications are satisfied.
/// 
/// Evaluation uses short circuit semantics right specification
/// is evaluated only if the left specification returns <c>true</c>.
/// Both specifications must remain side effect free.
/// </summary>
/// <typeparam name="T">Domain type the rule applies to</typeparam>
/// <param name="left">Left side of the conjunction</param>
/// <param name="right">Right side of the conjunction</param>
internal sealed class AndSpecification<T>(Specification<T> left, Specification<T> right) : Specification<T>
{
    /// <summary>
    /// Evaluates the conjunction of the two specifications.
    /// </summary>
    /// <param name="item">Instance to test</param>
    /// <returns>
    /// <c>true</c> only when both specifications return <c>true</c>;
    /// otherwise <c>false</c>.
    /// </returns>
    public override bool IsSatisfiedBy(T item) 
        => left.IsSatisfiedBy(item) && right.IsSatisfiedBy(item);
}