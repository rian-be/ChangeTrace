namespace ChangeTrace.Core.Specifications;

/// <summary>
/// Logical negation of another <see cref="Specification{T}"/>.
/// 
/// Inverts the evaluation result of the wrapped specification.
/// This specification is satisfied when the inner specification
/// is not satisfied.
/// 
/// Evaluation is delegated and must remain side effect free.
/// </summary>
/// <typeparam name="T">Domain type the rule applies to</typeparam>
/// <param name="spec">Specification to negate</param>
internal sealed class NotSpecification<T>(Specification<T> spec) : Specification<T>
{
    /// <summary>
    /// Evaluates the negated specification.
    /// </summary>
    /// <param name="item">Instance to test</param>
    /// <returns>
    /// <c>true</c> when the inner specification returns <c>false</c>;
    /// otherwise <c>false</c>.
    /// </returns>
    public override bool IsSatisfiedBy(T item) 
        => !spec.IsSatisfiedBy(item);
}