using System.Diagnostics.CodeAnalysis;

namespace FinancialManager.Application.Common;

/// <summary>
/// Outcome of an application operation that does not return a value.
/// Expected failures are returned instead of thrown so callers handle them explicitly.
/// </summary>
public class Result
{
    protected Result(Error? error)
    {
        Error = error;
    }

    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess => Error is null;

    public Error? Error { get; }

    public static Result Success() => new(null);

    public static Result Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new Result(error);
    }
}

/// <summary>
/// Outcome of an application operation that returns a value on success.
/// </summary>
public sealed class Result<T> : Result
{
    private readonly T? _value;

    private Result(T value)
        : base(null)
    {
        _value = value;
    }

    private Result(Error error)
        : base(error)
    {
    }

    /// <summary>
    /// The value produced by a successful operation.
    /// </summary>
    /// <exception cref="InvalidOperationException">The operation failed.</exception>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access the value of a failed result.");

#pragma warning disable CA1000 // Static factory members on generic types are intentional here.
    public static Result<T> Success(T value) => new(value);

    public static new Result<T> Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new Result<T>(error);
    }
#pragma warning restore CA1000
}
