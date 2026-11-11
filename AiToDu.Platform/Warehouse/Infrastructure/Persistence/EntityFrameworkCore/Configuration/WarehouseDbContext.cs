using AiToDu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using AiToDu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using AiToDu.Platform.Warehouse.Application.OutboundServices;
using AiToDu.Platform.Warehouse.Application.Exceptions;
using AiToDu.Platform.Warehouse.Domain.Model.Aggregates;
using AiToDu.Platform.Warehouse.Domain.Model.Errors;
using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
using AiToDu.Platform.Warehouse.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
namespace AiToDu.Platform.Warehouse.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

/// <summary>Warehouse integration extends the guide's unmodified shared context.</summary>
public sealed class WarehouseDbContext(DbContextOptions<WarehouseDbContext> options, ICurrentWarehouseActor? actor = null) : AppDbContext(options)
{
    public OrganizationId CurrentOrganizationId => actor?.OrganizationId ?? throw new InvalidOperationException("A verified organization is required to query inventory.");
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyWarehouseConfiguration();
        builder.Entity<InventoryItem>().HasQueryFilter(item => item.OrganizationId == CurrentOrganizationId);
        builder.UseSnakeCaseNamingConvention();
    }
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try { return await base.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException e) { throw new WarehousePersistenceConflictException(WarehouseErrorCode.VersionConflict,e); }
        catch (DbUpdateException e) when (e.InnerException is MySqlException { Number: 1062 } duplicate)
        { throw new WarehousePersistenceConflictException(duplicate.Message.Contains("ux_inventory_items_organization_sku", StringComparison.OrdinalIgnoreCase) ? WarehouseErrorCode.DuplicateSku : WarehouseErrorCode.PersistenceConflict,e); }
    }
}
