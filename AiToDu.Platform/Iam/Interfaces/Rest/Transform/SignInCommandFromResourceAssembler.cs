using AiToDu.Platform.Iam.Domain.Model.Commands;
using AiToDu.Platform.Iam.Interfaces.Rest.Resources;
namespace AiToDu.Platform.Iam.Interfaces.Rest.Transform;

public static class SignInCommandFromResourceAssembler
{
    public static SignInCommand ToCommandFromResource(SignInResource r) => new(r.Email, r.Password);
}
