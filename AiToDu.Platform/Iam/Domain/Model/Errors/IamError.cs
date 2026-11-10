namespace AiToDu.Platform.Iam.Domain.Model.Errors;

public enum IamError
{
    InvalidEmail, InvalidPassword, RequiredFields, DuplicateEmail, InvalidCredentials,
    AccountDisabled, Forbidden, UserNotFound, InvalidRole, CannotDisableSelf
}
