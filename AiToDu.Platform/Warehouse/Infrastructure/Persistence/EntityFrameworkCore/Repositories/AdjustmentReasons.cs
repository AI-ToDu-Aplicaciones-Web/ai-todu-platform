using AiToDu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using AiToDu.Platform.Warehouse.Application.CommandServices;
using AiToDu.Platform.Warehouse.Application.Exceptions;
using AiToDu.Platform.Warehouse.Application.OutboundServices;
using AiToDu.Platform.Warehouse.Domain.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace AiToDu.Platform.Warehouse.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public sealed class AdjustmentReasons(AppDbContext db, ICurrentWarehouseActor actor) : IAdjustmentReasons
{
    public Task<List<AdjustmentReason>> List(CancellationToken ct) => db.Set<AdjustmentReason>().AsNoTracking()
        .Where(x => x.OrganizationId == actor.OrganizationId.Value).OrderBy(x => x.Name).ToListAsync(ct);
    public async Task<AdjustmentReason?> Add(string name, CancellationToken ct)
    {
        var reason = new AdjustmentReason(actor.OrganizationId.Value, name);
        db.Add(reason);
        try { await db.SaveChangesAsync(ct); return reason; }
        catch (WarehousePersistenceConflictException) { return null; }
    }
}
