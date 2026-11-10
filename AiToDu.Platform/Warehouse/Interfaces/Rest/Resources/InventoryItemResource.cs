namespace AiToDu.Platform.Warehouse.Interfaces.Rest.Resources;

public sealed record InventoryItemResource(int Id, string Sku, string Name, string Category, decimal MinimumStock, decimal UnitPrice,
    long Version, decimal TotalQuantity, decimal AvailableQuantity, string Status, IReadOnlyList<StockLotResource> Lots,
    IReadOnlyList<InventoryMovementResource> Movements, IReadOnlyList<StockCountResource> Counts, IReadOnlyList<InventoryReturnResource> Returns);
