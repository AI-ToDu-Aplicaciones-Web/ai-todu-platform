using AiToDu.Platform.Iam.Application.Exceptions;
using AiToDu.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using AiToDu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using AiToDu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
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
        catch (DbUpdateException e) when (e.InnerException is MySqlException { Number: 1062 } duplicate && duplicate.Message.Contains("ux_users_email", StringComparison.OrdinalIgnoreCase))
        { throw new DuplicateEmailException(e); }
    }
}
