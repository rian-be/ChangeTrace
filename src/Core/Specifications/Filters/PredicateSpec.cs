using ChangeTrace.Core.Models;

namespace ChangeTrace.Core.Specifications.Filters;

/// <summary>
/// Represents a specification based on a <see cref="Func{TraceEvent, Boolean}"/>.
/// Allows wrapping any predicate as a <see cref="Specification{TraceEvent}"/>.
/// </summary>
/// <remarks>
/// This class is useful for creating dynamic or ad-hoc specifications
/// without creating a separate class for each condition.
/// </remarks>
/// <seealso cref="Specification{T}"/>
internal sealed class PredicateSpec(Func<TraceEvent, bool> predicate) : Specification<TraceEvent>
{
    /// <summary>
    /// The predicate function that defines specification.
    /// </summary>
    private readonly Func<TraceEvent, bool> _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));

    /// <summary>
    /// Determines whether the specified <paramref name="item"/> satisfies the predicate.
    /// </summary>
    /// <param name="item">The <see cref="TraceEvent"/> to evaluate.</param>
    /// <returns>
    /// <c>true</c> if the <paramref name="item"/> satisfies the predicate; otherwise, <c>false</c>.
    /// </returns>
    internal override bool IsSatisfiedBy(TraceEvent item) => _predicate(item);
}