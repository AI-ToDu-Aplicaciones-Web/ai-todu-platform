using AiToDu.Platform.Iam.Domain.Model.Commands;
using AiToDu.Platform.Iam.Interfaces.Rest.Resources;
namespace AiToDu.Platform.Iam.Interfaces.Rest.Transform;

public static class CreateOrganizationUserCommandFromResourceAssembler
{
    public static CreateOrganizationUserCommand ToCommandFromResource(CreateOrganizationUserResource r) => new(r.FullName, r.Email, r.Password, r.Role);
}
