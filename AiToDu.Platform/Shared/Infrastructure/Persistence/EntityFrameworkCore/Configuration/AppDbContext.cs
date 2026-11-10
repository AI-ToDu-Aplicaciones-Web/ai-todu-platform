using Microsoft.EntityFrameworkCore;
using AiToDu.Platform.Warehouse.Domain.Model.Aggregates;
using AiToDu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Models;
using AiToDu.Platform.Warehouse.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
namespace AiToDu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyWarehouseConfiguration();
        builder.Entity<OutboxMessage>(e => { e.ToTable("outbox_messages", "shared"); e.HasKey(x => x.Id); e.Property(x => x.Payload).HasColumnType("jsonb"); });
        foreach (var entity in builder.Model.GetEntityTypes())
            foreach (var property in entity.GetProperties())
                property.SetColumnName(System.Text.RegularExpressions.Regex.Replace(property.Name, "([a-z0-9])([A-Z])", "$1_$2").ToLowerInvariant());
    }
}
