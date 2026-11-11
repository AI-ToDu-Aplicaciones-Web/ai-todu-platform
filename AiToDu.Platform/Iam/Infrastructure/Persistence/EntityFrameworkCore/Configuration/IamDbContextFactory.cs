using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace AiToDu.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

public sealed class IamDbContextFactory : IDesignTimeDbContextFactory<IamDbContext>
{
    public IamDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build();
        var connection = configuration.GetConnectionString("DefaultConnection") ??
            "Server=127.0.0.1;Port=3306;Database=ai_todu;User ID=ai_todu";
        return new IamDbContext(new DbContextOptionsBuilder<IamDbContext>()
            .UseMySQL(connection, x => x.MigrationsHistoryTable("__ef_iam_migrations")).Options);
    }
}
