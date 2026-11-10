using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace AiToDu.Platform.Warehouse.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
public sealed class WarehouseDbContextFactory : IDesignTimeDbContextFactory<WarehouseDbContext>
{
    public WarehouseDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build();
        var connection = configuration.GetConnectionString("DefaultConnection") ?? "Host=127.0.0.1;Port=54339;Database=ai_todu_shared;Username=postgres";
        return new WarehouseDbContext(new DbContextOptionsBuilder<WarehouseDbContext>().UseNpgsql(connection, x => x.MigrationsHistoryTable("__ef_migrations_history","warehouse")).Options);
    }
}
