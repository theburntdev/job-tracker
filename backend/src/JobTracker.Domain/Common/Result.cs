namespace JobTracker.Domain.Common;

public sealed class Result<T>
{
    private readonly T? _value;
    private readonly string? _error;
    private readonly ErrorKind _kind;
    private readonly bool _isSuccess;

    private Result(T value)
    {
        _value = value;
        _isSuccess = true;
    }

    private Result(string error, ErrorKind kind)
    {
        _error = error;
        _kind = kind;
        _isSuccess = false;
    }

    public bool IsSuccess => _isSuccess;
    public bool IsFailure => !_isSuccess;

    public T Value => _isSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value on a failed Result.");

    public string Error => !_isSuccess
        ? _error!
        : throw new InvalidOperationException("Cannot access Error on a successful Result.");

    public ErrorKind Kind => !_isSuccess
        ? _kind
        : throw new InvalidOperationException("Cannot access Kind on a successful Result.");

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(string error, ErrorKind kind = ErrorKind.Validation) => new(error, kind);
}
