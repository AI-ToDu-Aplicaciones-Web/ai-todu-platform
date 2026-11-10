namespace AiToDu.Platform.Iam.Application.Exceptions;

public sealed class DuplicateEmailException(Exception innerException)
    : Exception("This email is already registered.", innerException);
