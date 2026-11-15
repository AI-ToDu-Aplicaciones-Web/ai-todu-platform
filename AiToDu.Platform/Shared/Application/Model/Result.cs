using AiTodu.Platform.Shared.Domain.Model.Errors;

namespace AiTodu.Platform.Shared.Application.Model;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public Error DomainError { get; }

    private Result(bool isSuccess, T? value, Error domainError)
    {
        IsSuccess = isSuccess;
        Value = value;
        DomainError = domainError;
    }

    public static Result<T> Success(T value) => new(true, value, Error.None);
    public static Result<T> Failure(Error error) => new(false, default, error);

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Error, TResult> onFailure) =>
        IsSuccess ? onSuccess(Value!) : onFailure(DomainError);
}