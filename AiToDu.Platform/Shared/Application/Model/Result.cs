namespace AiToDu.Platform.Shared.Application.Model;

public sealed class Result<T>
{
    public bool IsSuccess => Error is null;
    public T? Value { get; }
    public Error? Error { get; }
    private Result(T? value, Error? error) { Value = value; Error = error; }
    public static Result<T> Success(T value) => new(value, null);
    public static Result<T> Failure(string code, ErrorKind kind) => new(default, new Error(code, kind));
}
