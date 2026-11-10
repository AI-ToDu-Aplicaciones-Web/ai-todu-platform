using AiToDu.Platform.Warehouse.Domain.Model.Commands;
using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
using AiToDu.Platform.Warehouse.Interfaces.Rest.Resources;
namespace AiToDu.Platform.Warehouse.Interfaces.Rest.Transform;

public static class RelocateStockLotCommandFromResourceAssembler
{ public static RelocateStockLotCommand ToCommandFromResource(Guid id, Guid lotId, RelocateStockLotResource r) => new(new InventoryItemId(id), r.OperationId, r.ExpectedVersion, new StockLotId(lotId), r.Location, r.Reason); }
