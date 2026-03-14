using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ChangeTrace.Core.Results;

/// <summary>
/// Encapsulates the outcome of an operation, explicitly distinguishing
/// success from failure without throwing exceptions.
/// 
/// Supports optional error messages and exceptions. Designed for
/// simple, practical, no-ceremony error handling in domain and application code.
/// </summary>
internal readonly struct Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public Exception? Exception { get; }

    private Result(bool isSuccess, string? error, Exception? exception)
    {
        IsSuccess = isSuccess;
        Error = error;
        Exception = exception;
    }

    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsFailure
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => !IsSuccess;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result Success() => new(true, null, null);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result Failure(string error) => new(false, error, null);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result Failure(string error, Exception exception) => new(false, error, exception);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result OnSuccess(Action action)
    {
        if (IsSuccess) action();
        return this;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result OnFailure(Action<string> action)
    {
        if (IsFailure) action(Error!);
        return this;
    }

    public override string ToString() => IsSuccess ? "Success" : $"Failure: {Error}";
}