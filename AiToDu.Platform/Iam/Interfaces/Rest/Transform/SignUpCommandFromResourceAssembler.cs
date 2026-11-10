using AiToDu.Platform.Iam.Domain.Model.Commands;
using AiToDu.Platform.Iam.Interfaces.Rest.Resources;
namespace AiToDu.Platform.Iam.Interfaces.Rest.Transform;

public static class SignUpCommandFromResourceAssembler
{
    public static SignUpCommand ToCommandFromResource(SignUpResource r) => new(r.OrganizationName, r.FullName, r.Email, r.Password);
}
