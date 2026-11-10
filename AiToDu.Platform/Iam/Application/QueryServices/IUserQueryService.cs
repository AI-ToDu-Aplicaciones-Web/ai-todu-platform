using AiToDu.Platform.Iam.Domain.Model.Aggregates;
using AiToDu.Platform.Iam.Domain.Model.Queries;
using AiToDu.Platform.Shared.Application.Model;
namespace AiToDu.Platform.Iam.Application.QueryServices;

public interface IUserQueryService
{
    Task<Result<User>> Handle(GetCurrentUserQuery query, CancellationToken ct);
    Task<Result<IReadOnlyList<User>>> Handle(GetOrganizationUsersQuery query, CancellationToken ct);
}
