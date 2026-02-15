namespace ChangeTrace.Core.Results;

/// <summary>
/// Fluent extensions for <see cref="Result"/> and <see cref="Result{T}"/>.
/// Supports composition, validation, and safe value extraction.
/// </summary>
internal static class ResultExtensions
{
    /// <summary>
    /// Combines multiple results.
    /// 
    /// Returns the first failing result encountered,
    /// or success if all results succeeded.
    /// </summary>
    /// <param name="results">Collection of results to combine</param>
    public static Result Combine(this IEnumerable<Result> results)
    {
        foreach (var result in results)
        {
            if (result.IsFailure)
                return result;
        }
        return Result.Success();
    }

    extension<T>(Result<T> result)
    {
        /// <summary>
        /// Ensures the value of a successful result satisfies a predicate.
        /// Returns failure if the predicate returns false or if the result is already a failure.
        /// </summary>
        /// <typeparam name="T">Type of the result value</typeparam>
        /// <param name="predicate">Predicate to check the value</param>
        /// <param name="error">Failure message if the predicate is not satisfied</param>
        public Result<T> Ensure(Func<T, bool> predicate,
            string error)
        {
            if (result.IsFailure) return result;
            return predicate(result.Value) 
                ? result 
                : Result<T>.Failure(error);
        }

        /// <summary>
        /// Returns the value of a successful result, or a provided default if failed.
        /// </summary>
        /// <typeparam name="T">Type of the result value</typeparam>
        /// <param name="defaultValue">Value to return if result is failure</param>
        public T? GetValueOrDefault(T? defaultValue = default)
            => result.IsSuccess ? result.Value : defaultValue;
    }
}