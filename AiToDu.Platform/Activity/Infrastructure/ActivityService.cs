using AiToDu.Platform.Activity.Domain.Model;
using AiToDu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using AiToDu.Platform.Warehouse.Application.OutboundServices;
using AiToDu.Platform.Warehouse.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiToDu.Platform.Activity.Infrastructure;

public sealed class AuditMapping : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> e)
    {
        e.ToTable("audit_entries"); e.HasKey(x => x.Id);
        e.HasIndex(x => new { x.OrganizationId, x.OccurredAt }).HasDatabaseName("ix_audit_org_date");
    }
}
public sealed class ReadMapping : IEntityTypeConfiguration<NotificationRead>
{
    public void Configure(EntityTypeBuilder<NotificationRead> e)
    {
        e.ToTable("notification_reads"); e.HasKey(x => new { x.OrganizationId, x.ActorId, x.AlertKey });
        e.Property(x => x.AlertKey).HasMaxLength(100);
    }
}

public sealed class ActivityService(AppDbContext db, ICurrentWarehouseActor actor, IWarehouseClock clock)
{
    public async Task<List<StockAlert>> Alerts(CancellationToken ct)
    {
        var read = (await db.Set<NotificationRead>().Where(x => x.OrganizationId == actor.OrganizationId.Value &&
            x.ActorId == actor.ActorId.Value).Select(x => x.AlertKey).ToListAsync(ct)).ToHashSet();
        var items = await db.Set<InventoryItem>().AsNoTracking().IgnoreAutoIncludes().Include(x => x.Lots).ToListAsync(ct);
        var result = new List<StockAlert>();
        var today = DateOnly.FromDateTime(clock.GetCurrentTime().DateTime);
        foreach (var item in items.OrderBy(x => x.Sku.Value))
        {
            var quantity = item.AvailableQuantityOn(today);
            if (quantity <= item.MinimumStock.Value)
            {
                var key = $"stock-{item.Id.Value}";
                result.Add(new(key, "LowStock", item.Sku.Value, item.Id.Value, quantity, null, read.Contains(key)));
            }
            foreach (var lot in item.Lots.Where(l => l.Quantity.Value > 0 && l.ExpiresOn <= today.AddDays(30)))
            {
                var key = $"expiry-{item.Id.Value}-{lot.Id.Value:N}";
                result.Add(new(key, "Expiry", item.Sku.Value, item.Id.Value, lot.Quantity.Value, lot.ExpiresOn, read.Contains(key)));
            }
        }
        return result;
    }

    public async Task<bool> Read(string key, CancellationToken ct)
    {
        var alert = (await Alerts(ct)).SingleOrDefault(x => x.Key == key);
        if (alert == null) return false;
        if (!alert.IsRead)
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"INSERT INTO notification_reads (organization_id,actor_id,alert_key,read_at) VALUES ({actor.OrganizationId.Value},{actor.ActorId.Value},{key},{DateTime.UtcNow}) ON DUPLICATE KEY UPDATE read_at=read_at", ct);
        return true;
    }

    public Task<List<AuditEntry>> History(CancellationToken ct) => db.Set<AuditEntry>().AsNoTracking()
        .Where(x => x.OrganizationId == actor.OrganizationId.Value).OrderByDescending(x => x.OccurredAt).Take(200).ToListAsync(ct);
}
