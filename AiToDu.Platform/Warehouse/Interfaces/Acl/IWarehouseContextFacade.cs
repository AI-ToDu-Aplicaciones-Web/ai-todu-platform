namespace AiToDu.Platform.Warehouse.Interfaces.Acl;

/// <summary>Published contract for Sales/Logistics. No warehouse entities cross this boundary.</summary>
public sealed record AvailableLot(Guid Id, decimal Quantity);
public sealed record SaleQuote(int ItemId, string Sku, string Name, decimal UnitPrice, long Version, IReadOnlyList<AvailableLot> Lots);

public interface IWarehouseContextFacade
{
    Task<SaleQuote?> Quote(int itemId, CancellationToken ct);
    Task<bool> AcceptSaleReturn(int itemId, Guid lot, Guid operation, decimal quantity, string disposition, string reference, string reason, CancellationToken ct);
    Task<decimal?> FetchAvailableQuantity(int inventoryItemId, CancellationToken ct);
    Task<bool> IssueForSale(int inventoryItemId, Guid lotId, Guid saleOperationId, long expectedVersion, decimal quantity, string saleReference, CancellationToken ct);
}
