using AiToDu.Platform.Warehouse.Domain.Model.Commands;
using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
using AiToDu.Platform.Warehouse.Interfaces.Rest.Resources;
namespace AiToDu.Platform.Warehouse.Interfaces.Rest.Transform;

public static class ChangeLotQuarantineCommandFromResourceAssembler
{ public static ChangeLotQuarantineCommand ToCommandFromResource(Guid id, Guid lotId, ChangeLotQuarantineResource r) => new(new InventoryItemId(id), r.OperationId, r.ExpectedVersion, new StockLotId(lotId), r.IsQuarantined, r.Reason); }
