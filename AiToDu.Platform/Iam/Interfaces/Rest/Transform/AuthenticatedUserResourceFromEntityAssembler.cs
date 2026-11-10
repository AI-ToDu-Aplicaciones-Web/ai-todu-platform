using AiToDu.Platform.Iam.Application.Model;
using AiToDu.Platform.Iam.Interfaces.Rest.Resources;
namespace AiToDu.Platform.Iam.Interfaces.Rest.Transform;

public static class AuthenticatedUserResourceFromEntityAssembler
{
    public static AuthenticatedUserResource ToResourceFromEntity(AuthenticatedUser session) =>
        new(UserResourceFromEntityAssembler.ToResourceFromEntity(session.User), session.Token, session.ExpiresAt);
}
