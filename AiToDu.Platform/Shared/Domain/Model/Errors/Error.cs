namespace AiToDu.Platform.Shared.Domain.Model.Errors
{
    public enum EErrorType
    {
        Failure = 0,
        Validation = 1,
        NotFound = 2,
        Conflict = 3,
        Unauthorized = 4,
        Forbidden = 5,
        Unexpected = 6
    }

    public record Error(string Code, string Message, EErrorType Type = EErrorType.Failure)
    {
        public static readonly Error None = new(string.Empty, string.Empty, EErrorType.Failure);

        public static Error Failure(string code, string message) => new(code, message, EErrorType.Failure);
        public static Error Validation(string code, string message) => new(code, message, EErrorType.Validation);
        public static Error NotFound(string code, string message) => new(code, message, EErrorType.NotFound);
        public static Error Conflict(string code, string message) => new(code, message, EErrorType.Conflict);
        public static Error Unauthorized(string code, string message) => new(code, message, EErrorType.Unauthorized);
        public static Error Forbidden(string code, string message) => new(code, message, EErrorType.Forbidden);
        public static Error Unexpected(string code, string message) => new(code, message, EErrorType.Unexpected);
    }
}

namespace AiTodu.Platform.Shared.Domain.Model.Errors
{
    // Alias compatible por si algún archivo importa AiTodu (con 'd' minúscula)
    public class SharedErrorsMarker;
}