using AiToDu.Platform.Shared.Application.Model;
using AiToDu.Platform.Warehouse.Domain.Model.Aggregates;
using AiToDu.Platform.Warehouse.Domain.Model.Commands;

namespace AiToDu.Platform.Warehouse.Application.CommandServices;

public interface IInventoryItemCommandService
{
    Task<Result<InventoryItem>> Handle(IssueStockForSaleCommand command, CancellationToken ct);
    Task<Result<InventoryItem>> Handle(CreateInventoryItemCommand command, CancellationToken ct);
    Task<Result<InventoryItem>> Handle(ReceiveStockLotCommand command, CancellationToken ct);
    Task<Result<InventoryItem>> Handle(RecordInventoryMovementCommand command, CancellationToken ct);
    Task<Result<InventoryItem>> Handle(RegisterStockCountCommand command, CancellationToken ct);
    Task<Result<InventoryItem>> Handle(ClassifyInventoryReturnCommand command, CancellationToken ct);
    Task<Result<InventoryItem>> Handle(ChangeLotQuarantineCommand command, CancellationToken ct);
    Task<Result<InventoryItem>> Handle(RelocateStockLotCommand command, CancellationToken ct);
}
