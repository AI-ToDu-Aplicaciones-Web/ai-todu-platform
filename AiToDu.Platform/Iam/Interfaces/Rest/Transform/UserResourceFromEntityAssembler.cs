using AiToDu.Platform.Iam.Domain.Model.Aggregates;
using AiToDu.Platform.Iam.Interfaces.Rest.Resources;
namespace AiToDu.Platform.Iam.Interfaces.Rest.Transform;

public static class UserResourceFromEntityAssembler
{
    public static UserResource ToResourceFromEntity(User user) => new(user.PublicId, user.OrganizationId,
        user.OrganizationName, user.FullName, user.Email.Value, user.Role, user.IsActive, user.CanManageStock, user.CanSell);
}
