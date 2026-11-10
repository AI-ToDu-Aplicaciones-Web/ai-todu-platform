using AiToDu.Platform.Warehouse.Domain.Model.Errors;
namespace AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;

public readonly record struct InventoryItemId
{
    public Guid Value { get; }
    public InventoryItemId(Guid value)
    {
        if (value == Guid.Empty) throw new WarehouseDomainException(WarehouseErrorCode.InvalidIdentifier);
        Value = value;
    }
    public static InventoryItemId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}
