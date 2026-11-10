using AiToDu.Platform.Shared.Domain.Model.Errors;
namespace AiToDu.Platform.Iam.Domain.Model.Errors;

public static class IamErrors
{
    public static Error From(IamError code) => code switch
    {
        IamError.DuplicateEmail => Error.Conflict($"Iam.{code}", "This email is already registered."),
        IamError.InvalidCredentials => Error.Unauthorized($"Iam.{code}", "The email or password is incorrect."),
        IamError.AccountDisabled => Error.Forbidden($"Iam.{code}", "This account is disabled."),
        IamError.Forbidden => Error.Forbidden($"Iam.{code}", "You do not have permission for this action."),
        IamError.UserNotFound => Error.NotFound($"Iam.{code}", "The user was not found."),
        _ => Error.Validation($"Iam.{code}", "Review the supplied account details.")
    };
}
