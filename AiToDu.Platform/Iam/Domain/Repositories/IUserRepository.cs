using AiToDu.Platform.Iam.Domain.Model.Aggregates;
using AiToDu.Platform.Iam.Domain.Model.ValueObjects;
using AiToDu.Platform.Shared.Domain.Repositories;
namespace AiToDu.Platform.Iam.Domain.Repositories;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> FindByEmailAsync(EmailAddress email, CancellationToken ct);
    Task<User?> FindByPublicIdAsync(Guid publicId, CancellationToken ct);
    Task<IReadOnlyList<User>> FindByOrganizationAsync(Guid organizationId, CancellationToken ct);
}
