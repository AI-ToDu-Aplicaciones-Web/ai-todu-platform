using AiToDu.Platform.Iam.Application.Internal.OutboundServices;
using AiToDu.Platform.Iam.Application.QueryServices;
using AiToDu.Platform.Iam.Domain.Model.Aggregates;
using AiToDu.Platform.Iam.Domain.Model.Errors;
using AiToDu.Platform.Iam.Domain.Model.Queries;
using AiToDu.Platform.Iam.Domain.Repositories;
using AiToDu.Platform.Shared.Application.Model;
namespace AiToDu.Platform.Iam.Application.Internal.QueryServices;

public sealed class UserQueryService(IUserRepository repository, ICurrentIamUser currentUser) : IUserQueryService
{
    public async Task<Result<User>> Handle(GetCurrentUserQuery query, CancellationToken ct)
    {
        var user = await repository.FindByPublicIdAsync(currentUser.PublicId, ct);
        return user is not null && user.IsActive && user.OrganizationId == currentUser.OrganizationId
            ? Result<User>.Success(user)
            : Result<User>.Failure(IamErrors.From(IamError.UserNotFound), IamError.UserNotFound);
    }

    public async Task<Result<IReadOnlyList<User>>> Handle(GetOrganizationUsersQuery query, CancellationToken ct)
    {
        if (!currentUser.IsOwner) return Result<IReadOnlyList<User>>.Failure(IamErrors.From(IamError.Forbidden), IamError.Forbidden);
        return Result<IReadOnlyList<User>>.Success(await repository.FindByOrganizationAsync(currentUser.OrganizationId, ct));
    }
}
