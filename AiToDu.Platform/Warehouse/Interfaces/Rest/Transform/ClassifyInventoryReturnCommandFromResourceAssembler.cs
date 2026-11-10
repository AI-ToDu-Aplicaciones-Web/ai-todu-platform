using AiToDu.Platform.Warehouse.Domain.Model.Commands;
using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
using AiToDu.Platform.Warehouse.Interfaces.Rest.Resources;
namespace AiToDu.Platform.Warehouse.Interfaces.Rest.Transform;

public static class ClassifyInventoryReturnCommandFromResourceAssembler
{ public static ClassifyInventoryReturnCommand ToCommandFromResource(int id, ClassifyInventoryReturnResource r) => new(new InventoryItemId(id), r.OperationId, r.ExpectedVersion, new StockLotId(r.SourceLotId), r.Quantity, r.Disposition, r.Reference, r.Reason); }
