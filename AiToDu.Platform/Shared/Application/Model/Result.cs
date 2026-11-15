using System;
using DomainErrorModel = AiToDu.Platform.Shared.Domain.Model.Errors.Error;

namespace AiToDu.Platform.Shared.Application.Model
{
    /// <summary>
    ///     Generic Result class for Command and Query Handlers in the Application Layer.
    /// </summary>
    public class Result<T>
    {
        protected Result(bool isSuccess, T? value, string message, Enum? errorEnum, DomainErrorModel? error)
        {
            IsSuccess = isSuccess;
            Value = value;
            Message = message;
            Error = errorEnum;
            DomainError = error ?? (isSuccess
                ? DomainErrorModel.None
                : DomainErrorModel.Failure(errorEnum?.ToString() ?? "Error", message));
        }

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public T? Value { get; }
        public string Message { get; }
        public Enum? Error { get; }
        public DomainErrorModel DomainError { get; }

        public static Result<T> Success(T value) =>
            new(true, value, string.Empty, null, DomainErrorModel.None);

        public static Result<T> Failure(Enum error, string message) =>
            new(false, default, message, error, DomainErrorModel.Failure(error.ToString(), message));

        public static Result<T> Failure(DomainErrorModel error) =>
            new(false, default, error.Message, null, error);

        public static Result<T> Failure(DomainErrorModel error, Enum errorEnum) =>
            new(false, default, error.Message, errorEnum, error);

        public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<DomainErrorModel, TResult> onFailure) =>
            IsSuccess ? onSuccess(Value!) : onFailure(DomainError);

        public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<DomainErrorModel, Enum?, TResult> onFailure) =>
            IsSuccess ? onSuccess(Value!) : onFailure(DomainError, Error);
    }

    /// <summary>
    ///     Non-generic Result class for Command Handlers.
    /// </summary>
    public class Result : Result<object>
    {
        private Result(bool isSuccess, string message, Enum? errorEnum, DomainErrorModel? error)
            : base(isSuccess, null, message, errorEnum, error)
        {
        }

        public static Result Success() =>
            new(true, string.Empty, null, DomainErrorModel.None);

        public new static Result Failure(Enum error, string message) =>
            new(false, message, error, DomainErrorModel.Failure(error.ToString(), message));

        public new static Result Failure(DomainErrorModel error) =>
            new(false, error.Message, null, error);

        public new static Result Failure(DomainErrorModel error, Enum errorEnum) =>
            new(false, error.Message, errorEnum, error);
    }
}

namespace AiTodu.Platform.Shared.Application.Model
{
    using AiToDu.Platform.Shared.Domain.Model.Errors;

    public class Result<T> : AiToDu.Platform.Shared.Application.Model.Result<T>
    {
        protected Result(bool isSuccess, T? value, string message, Enum? errorEnum, Error? error)
            : base(isSuccess, value, message, errorEnum, error)
        {
        }

        public new static Result<T> Success(T value) => new(true, value, string.Empty, null, AiToDu.Platform.Shared.Domain.Model.Errors.Error.None);
        public new static Result<T> Failure(Enum error, string message) => new(false, default, message, error, AiToDu.Platform.Shared.Domain.Model.Errors.Error.Failure(error.ToString(), message));
        public new static Result<T> Failure(Error error) => new(false, default, error.Message, null, error);
        public new static Result<T> Failure(Error error, Enum errorEnum) => new(false, default, error.Message, errorEnum, error);
    }
}