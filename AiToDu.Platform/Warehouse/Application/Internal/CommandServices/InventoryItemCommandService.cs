using AiToDu.Platform.Shared.Application.Model;
using AiToDu.Platform.Warehouse.Domain.Model.Aggregates;
using AiToDu.Platform.Warehouse.Domain.Model.Commands;
using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
using AiToDu.Platform.Warehouse.Domain.Model.Errors;
using AiToDu.Platform.Shared.Domain.Model.ValueObjects;

using AiToDu.Platform.Shared.Application.Exceptions;
using AiToDu.Platform.Shared.Domain.Repositories;
using AiToDu.Platform.Warehouse.Application.CommandServices;
using AiToDu.Platform.Warehouse.Application.OutboundServices;
using AiToDu.Platform.Warehouse.Domain.Repositories;

namespace AiToDu.Platform.Warehouse.Application.Internal.CommandServices;

public sealed class InventoryItemCommandService(IInventoryItemRepository repository, IUnitOfWork unitOfWork,
    ICurrentWarehouseActor actor, IWarehouseClock clock) : IInventoryItemCommandService
{
    public async Task<Result<InventoryItem>> Handle(CreateInventoryItemCommand c, CancellationToken ct)
    {
        if (!actor.CanManageInventory) return Result<InventoryItem>.Failure("Forbidden", ErrorKind.Forbidden);
        try
        {
            var sku = new Sku(c.Sku);
            if (await repository.SkuExistsAsync(actor.OrganizationId, sku, ct)) return Result<InventoryItem>.Failure("DuplicateSku", ErrorKind.Conflict);
            var item = new InventoryItem(InventoryItemId.New(), actor.OrganizationId, sku, c.Name, c.Category, new StockQuantity(c.MinimumStock), c.UnitPrice, actor.ActorId, clock.GetCurrentTime());
            await repository.AddAsync(item, ct);
            await unitOfWork.CompleteAsync(ct);
            return Result<InventoryItem>.Success(item);
        }
        catch (WarehouseDomainException e) { return Rejected(e); }
        catch (PersistenceConflictException e) { return Result<InventoryItem>.Failure(e.Code, ErrorKind.Conflict); }
    }
    public Task<Result<InventoryItem>> Handle(IssueStockForSaleCommand c, CancellationToken ct) =>
        Execute(c.ItemId, item => item.RecordMovement(c.OperationId, c.ExpectedVersion, c.LotId, MovementType.Issue, c.Quantity, new MovementReason(c.SaleReference), actor.ActorId, clock.GetCurrentTime()), ct, true);
    private async Task<Result<InventoryItem>> Execute(InventoryItemId itemId, Action<InventoryItem> action, CancellationToken ct, bool salesIssue = false)
    {
        if (!(salesIssue ? actor.CanRecordSales : actor.CanManageInventory)) return Result<InventoryItem>.Failure("Forbidden", ErrorKind.Forbidden);
        try
        {
            var item = await repository.FindAsync(actor.OrganizationId, itemId, true, ct);
            if (item is null) return Result<InventoryItem>.Failure("ItemNotFound", ErrorKind.NotFound);
            var previousVersion = item.Version;
            action(item);
            if (item.Version != previousVersion) await unitOfWork.CompleteAsync(ct);
            return Result<InventoryItem>.Success(item);
        }
        catch (WarehouseDomainException e) { return Rejected(e); }
        catch (PersistenceConflictException e) { return Result<InventoryItem>.Failure(e.Code, ErrorKind.Conflict); }
    }
    private static Result<InventoryItem> Rejected(WarehouseDomainException e) => Result<InventoryItem>.Failure(e.Code,
        e.Code.EndsWith("NotFound") ? ErrorKind.NotFound : e.IsConflict ? ErrorKind.Conflict : ErrorKind.Validation);
    public Task<Result<InventoryItem>> Handle(ReceiveStockLotCommand c, CancellationToken ct) => Execute(c.ItemId, item => item.ReceiveLot(c.OperationId, c.ExpectedVersion, new LotCode(c.LotCode), new StorageLocation(c.Location), c.ExpiresOn, new StockQuantity(c.Quantity), actor.ActorId, clock.GetCurrentTime()), ct);
    public Task<Result<InventoryItem>> Handle(RecordInventoryMovementCommand c, CancellationToken ct) => Execute(c.ItemId, item => item.RecordMovement(c.OperationId, c.ExpectedVersion, c.LotId, c.Type, c.Quantity, new MovementReason(c.Reason), actor.ActorId, clock.GetCurrentTime()), ct);
    public Task<Result<InventoryItem>> Handle(RegisterStockCountCommand c, CancellationToken ct) => Execute(c.ItemId, item => item.RegisterCount(c.OperationId, c.ExpectedVersion, c.LotId, new StockQuantity(c.ExpectedQuantity), new StockQuantity(c.CountedQuantity), new MovementReason(c.Reason), actor.ActorId, clock.GetCurrentTime()), ct);
    public Task<Result<InventoryItem>> Handle(ClassifyInventoryReturnCommand c, CancellationToken ct) => Execute(c.ItemId, item => item.ClassifyReturn(c.OperationId, c.ExpectedVersion, c.SourceLotId, new StockQuantity(c.Quantity), c.Disposition, c.Reference, new MovementReason(c.Reason), actor.ActorId, clock.GetCurrentTime()), ct);
    public Task<Result<InventoryItem>> Handle(ChangeLotQuarantineCommand c, CancellationToken ct) => Execute(c.ItemId, item => item.SetQuarantine(c.OperationId, c.ExpectedVersion, c.LotId, c.IsQuarantined, new MovementReason(c.Reason), actor.ActorId, clock.GetCurrentTime()), ct);
    public Task<Result<InventoryItem>> Handle(RelocateStockLotCommand c, CancellationToken ct) => Execute(c.ItemId, item => item.RelocateLot(c.OperationId, c.ExpectedVersion, c.LotId, new StorageLocation(c.Location), new MovementReason(c.Reason), actor.ActorId, clock.GetCurrentTime()), ct);
}
