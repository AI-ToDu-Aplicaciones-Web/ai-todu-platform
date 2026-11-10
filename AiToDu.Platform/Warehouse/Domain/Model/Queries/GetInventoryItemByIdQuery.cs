using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
namespace AiToDu.Platform.Warehouse.Domain.Model.Queries;

public sealed record GetInventoryItemByIdQuery(InventoryItemId ItemId);
