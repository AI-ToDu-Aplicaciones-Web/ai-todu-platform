using AiToDu.Platform.Iam.Domain.Model.Aggregates;
using AiToDu.Platform.Iam.Domain.Model.ValueObjects;
using AiToDu.Platform.Iam.Domain.Repositories;
using AiToDu.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using AiToDu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;
namespace AiToDu.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public sealed class UserRepository(IamDbContext context) : BaseRepository<User>(context), IUserRepository
{
    public Task<User?> FindByEmailAsync(EmailAddress email, CancellationToken ct) =>
        Context.Set<User>().SingleOrDefaultAsync(x => x.Email == email, ct);
    public Task<User?> FindByPublicIdAsync(Guid publicId, CancellationToken ct) =>
        Context.Set<User>().SingleOrDefaultAsync(x => x.PublicId == publicId, ct);
    public async Task<IReadOnlyList<User>> FindByOrganizationAsync(Guid organizationId, CancellationToken ct) =>
        await Context.Set<User>().AsNoTracking().Where(x => x.OrganizationId == organizationId).OrderBy(x => x.FullName).ToListAsync(ct);
}
