using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
using AiToDu.Platform.Warehouse.Domain.Model.Errors;
using AiToDu.Platform.Shared.Domain.Model.ValueObjects;

namespace AiToDu.Platform.Warehouse.Domain.Model.Entities;

/// <summary>Entity owned exclusively by InventoryItem. It has no independent repository.</summary>
public sealed class StockLot
{
    public StockLotId Id { get; private set; }
    public LotCode Code { get; private set; } = null!;
    public StorageLocation Location { get; private set; } = null!;
    public DateOnly? ExpiresOn { get; private set; }
    public StockQuantity Quantity { get; private set; }
    public bool IsQuarantined { get; private set; }
    private StockLot() { }
    internal StockLot(StockLotId id, LotCode code, StorageLocation location, DateOnly? expiresOn, bool quarantine)
    {
        Id = id; Code = code; Location = location; ExpiresOn = expiresOn; IsQuarantined = quarantine;
        Quantity = new StockQuantity(0);
    }
    internal bool IsAvailableOn(DateOnly date) => !IsQuarantined && (ExpiresOn is null || ExpiresOn >= date);
    internal void ChangeQuantity(decimal value) => Quantity = new StockQuantity(value);
    internal void Relocate(StorageLocation location) => Location = location;
    internal void SetQuarantine(bool value) => IsQuarantined = value;
}
