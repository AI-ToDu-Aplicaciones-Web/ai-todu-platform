using AiToDu.Platform.Iam.Domain.Model.Aggregates;
using AiToDu.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using AiToDu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using AiToDu.Platform.Subscriptions.Domain.Model;
using AiToDu.Platform.Warehouse.Domain.Model.Aggregates;
using AiToDu.Platform.Warehouse.Application.OutboundServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiToDu.Platform.Subscriptions.Infrastructure;

public sealed class SubscriptionMapping : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> model)
    {
        model.ToTable("subscriptions"); model.HasKey(x => x.OrganizationId);
        model.Property(x => x.Plan).HasMaxLength(20);
    }
}

public sealed class SubscriptionService(AppDbContext db, IamDbContext iam, ICurrentWarehouseActor actor)
{
    private Guid Organization => actor.OrganizationId.Value;
    public async Task<Subscription> Current(CancellationToken ct) =>
        await db.Set<Subscription>().FindAsync([Organization], ct) ?? new Subscription(Organization);
    public Task<int> Users(CancellationToken ct) => iam.Set<User>().CountAsync(x => x.OrganizationId == Organization, ct);
    public Task<int> Products(CancellationToken ct) => db.Set<InventoryItem>().CountAsync(ct);
    public async Task<bool> Choose(string plan, CancellationToken ct)
    {
        var sub = await Current(ct);
        if (!sub.Choose(plan, await Users(ct), await Products(ct))) return false;
        if (db.Entry(sub).State == EntityState.Detached) db.Add(sub);
        await db.SaveChangesAsync(ct);
        return true;
    }

    // MySQL's connection-scoped lock serializes allocations across both DbContexts and all API instances.
    public async Task<bool> WithLock(Func<Task> action, CancellationToken ct)
    {
        await db.Database.OpenConnectionAsync(ct);
        await using var command = db.Database.GetDbConnection().CreateCommand();
        var name = command.CreateParameter(); name.ParameterName = "@name"; name.Value = $"plan:{Organization:N}";
        command.Parameters.Add(name);
        try
        {
            command.CommandText = "SELECT GET_LOCK(@name, 8)";
            if (Convert.ToInt32(await command.ExecuteScalarAsync(ct)) != 1) return false;
            try { await action(); }
            finally { command.CommandText = "SELECT RELEASE_LOCK(@name)"; await command.ExecuteScalarAsync(CancellationToken.None); }
            return true;
        }
        finally { await db.Database.CloseConnectionAsync(); }
    }
}
