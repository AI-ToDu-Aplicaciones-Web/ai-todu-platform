using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
using AiToDu.Platform.Warehouse.Domain.Model.Errors;
using Microsoft.EntityFrameworkCore;
using AiToDu.Platform.Warehouse.Domain.Model.Aggregates;
using AiToDu.Platform.Warehouse.Domain.Repositories;
using AiToDu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using AiToDu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
namespace AiToDu.Platform.Warehouse.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public sealed class InventoryItemRepository(AppDbContext context) : BaseRepository<InventoryItem>(context), IInventoryItemRepository
{
    private IQueryable<InventoryItem> Complete(IQueryable<InventoryItem> query) => query
        .Include(x => x.Lots).Include(x => x.Movements).Include(x => x.Counts).Include(x => x.Returns).Include(x => x.Operations).AsSplitQuery();
    public Task<InventoryItem?> FindAsync(OrganizationId organizationId, InventoryItemId itemId, bool tracking, CancellationToken ct)
    {
        var query = Context.Set<InventoryItem>().Where(x => x.OrganizationId == organizationId && x.Id == itemId);
        if (!tracking) query = query.AsNoTracking();
        return Complete(query).SingleOrDefaultAsync(ct);
    }
    public Task<InventoryItem?> FindBySku(OrganizationId organizationId, Sku sku, CancellationToken ct) =>
        Complete(Context.Set<InventoryItem>().AsNoTracking()).SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.Sku == sku, ct);
    public Task<bool> SkuExistsAsync(OrganizationId organizationId, Sku sku, CancellationToken ct) => Context.Set<InventoryItem>().AnyAsync(x => x.OrganizationId == organizationId && x.Sku == sku, ct);
    public async Task<(IReadOnlyList<InventoryItem> Items, int Total)> SearchAsync(OrganizationId organizationId, string? search, int page, int pageSize, CancellationToken ct)
    {
        var query = Context.Set<InventoryItem>().AsNoTracking().Where(x => x.OrganizationId == organizationId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var name = search.Trim().ToLowerInvariant();
            var sku = search.Trim().Length <= 64 ? new Sku(search) : null;
            query = query.Where(x => x.Name.ToLower().Contains(name) || (sku != null && x.Sku == sku));
        }
        var count = await query.CountAsync(ct);
        var items = await Complete(query.OrderBy(x => x.Name).ThenBy(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize)).ToListAsync(ct);
        return (items, count);
    }
}
