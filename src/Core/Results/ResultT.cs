using System.Diagnostics.CodeAnalysis;

namespace ChangeTrace.Core.Results;

/// <summary>
/// Represents the outcome of an operation that returns value of type <typeparamref name="T"/>.
/// 
/// Provides explicit success/failure handling without exceptions, supporting
/// fluent transformations, binding, and functional-style error propagation.
/// </summary>
/// <typeparam name="T">Type of the successful result value</typeparam>
internal readonly struct Result<T>
{
    private readonly T _value;

    public bool IsSuccess { get; }
    public string? Error { get; }
    private Exception? Exception { get; }

    private Result(bool isSuccess, T value, string? error, Exception? exception)
    {
        IsSuccess = isSuccess;
        _value = value;
        Error = error;
        Exception = exception;
    }

    [MemberNotNullWhen(true, nameof(_value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsFailure => !IsSuccess;

    public T Value => IsSuccess ? _value : throw new InvalidOperationException(Error, Exception);

    public T? ValueOrNull => IsSuccess ? _value : default;

    public static Result<T> Success(T value) => new(true, value, null, null);

    public static Result<T> Failure(string error, Exception? exception = null) 
        => new(false, default!, error, exception);

    /// <summary>
    /// Transforms the successful value to a new type.
    /// If result is failure, propagates the failure.
    /// Exceptions thrown in <paramref name="mapper"/> are captured as failures.
    /// </summary>
    public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
    {
        if (IsFailure) 
            return Result<TNew>.Failure(Error!, Exception);

        try
        {
            return Result<TNew>.Success(mapper(_value));
        }
        catch (Exception ex)
        {
            return Result<TNew>.Failure("Mapping failed", ex);
        }
    }

    /// <summary>
    /// Binds the successful value to another <see cref="Result{TNew}"/>.
    /// Propagates failure if the current result failed.
    /// </summary>
    public Result<TNew> Bind<TNew>(Func<T, Result<TNew>> binder)
        => IsFailure ? Result<TNew>.Failure(Error!, Exception) : binder(_value);

    /// <summary>
    /// Asynchronously binds the successful value to another <see cref="Result{TNew}"/>.
    /// Propagates failure if the current result failed.
    /// </summary>
    public async Task<Result<TNew>> BindAsync<TNew>(Func<T, Task<Result<TNew>>> binder)
        => IsFailure ? Result<TNew>.Failure(Error!, Exception) : await binder(_value);

    public Result<T> OnSuccess(Action<T> action)
    {
        if (IsSuccess) action(_value);
        return this;
    }

    public Result<T> OnFailure(Action<string> action)
    {
        if (IsFailure) action(Error!);
        return this;
    }

    /// <summary>
    /// Matches over the result, executing <paramref name="onSuccess"/> if successful,
    /// or <paramref name="onFailure"/> if failed.
    /// </summary>
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
        => IsSuccess ? onSuccess(_value) : onFailure(Error!);

    public override string ToString() => IsSuccess ? $"Success: {_value}" : $"Failure: {Error}";

    public static implicit operator Result<T>(T value) => Success(value);
}