using System.Text;
using AiToDu.Platform.Iam.Domain.Model.Errors;
namespace AiToDu.Platform.Iam.Domain.Model.ValueObjects;

public static class PasswordPolicy
{
    // BCrypt accepts at most 72 UTF-8 bytes. Reject longer input instead of truncating it.
    public static void Validate(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 12 ||
            Encoding.UTF8.GetByteCount(password) > 72)
            throw new IamDomainException(IamError.InvalidPassword);
    }
}
