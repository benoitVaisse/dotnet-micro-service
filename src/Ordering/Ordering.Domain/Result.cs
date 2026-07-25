namespace Ordering.Domain;

public record Result
{
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }

    protected Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        ErrorMessage = error;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(string ErrorMessage) => new(false, ErrorMessage ?? throw new ArgumentNullException(nameof(ErrorMessage)));

}

public record Result<T> : Result
{
    public T? Value { get; }

    private Result(T value) : base(true, null) => Value = value;
    private Result(string error) : base(false, error) { }

    public static Result<T> Success(T value) => new(value);
    public static new Result<T> Failure(string error) => new(error);
}