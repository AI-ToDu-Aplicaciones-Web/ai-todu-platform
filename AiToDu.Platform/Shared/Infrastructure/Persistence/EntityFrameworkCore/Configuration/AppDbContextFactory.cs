using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace AiToDu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build();
        var connection = configuration.GetConnectionString("DefaultConnection") ?? "Host=127.0.0.1;Port=54339;Database=ai_todu;Username=postgres";
        return new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(connection, x => x.MigrationsHistoryTable("__ef_migrations_history", "warehouse")).Options);
    }
}
