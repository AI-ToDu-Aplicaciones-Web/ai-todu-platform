using AiToDu.Platform.Warehouse.Domain.Model.Commands;
using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
using AiToDu.Platform.Warehouse.Interfaces.Rest.Resources;
namespace AiToDu.Platform.Warehouse.Interfaces.Rest.Transform;

public static class ReceiveStockLotCommandFromResourceAssembler
{ public static ReceiveStockLotCommand ToCommandFromResource(int id, ReceiveStockLotResource r) => new(new InventoryItemId(id), r.OperationId, r.ExpectedVersion, r.LotCode, r.Location, r.ExpiresOn, r.Quantity); }
