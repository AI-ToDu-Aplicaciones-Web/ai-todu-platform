namespace AiToDu.Platform.Warehouse.Interfaces.Acl;

/// <summary>Published contract for Sales/Logistics. No warehouse entities cross this boundary.</summary>
public interface IWarehouseContextFacade
{
    Task<decimal?> FetchAvailableQuantity(Guid inventoryItemId, CancellationToken ct);
    Task<bool> IssueForSale(Guid inventoryItemId, Guid lotId, Guid saleOperationId, long expectedVersion, decimal quantity, string saleReference, CancellationToken ct);
}
