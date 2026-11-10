using AiToDu.Platform.Iam.Application.Exceptions;
using AiToDu.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using AiToDu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using AiToDu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
namespace AiToDu.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

public sealed class IamDbContext(DbContextOptions<IamDbContext> options) : AppDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyIamConfiguration();
        builder.UseSnakeCaseNamingConvention();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try { return await base.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: "23505", ConstraintName: "ux_users_email" })
        { throw new DuplicateEmailException(e); }
    }
}
