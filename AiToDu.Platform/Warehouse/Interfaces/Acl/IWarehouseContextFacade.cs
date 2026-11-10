namespace AiToDu.Platform.Warehouse.Interfaces.Acl;

/// <summary>Published contract for Sales/Logistics. No warehouse entities cross this boundary.</summary>
public interface IWarehouseContextFacade
{
    Task<decimal?> FetchAvailableQuantity(int inventoryItemId, CancellationToken ct);
    Task<bool> IssueForSale(int inventoryItemId, Guid lotId, Guid saleOperationId, long expectedVersion, decimal quantity, string saleReference, CancellationToken ct);
}
