using AiToDu.Platform.Shared.Domain.Model.ValueObjects;
namespace AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
public readonly record struct InventoryItemId(int Value) : IEntityId<int>
{
    public static InventoryItemId New() => new(0);
    public static implicit operator int(InventoryItemId id) => id.Value;
    public static implicit operator InventoryItemId(int value) => new(value);
    public override string ToString() => Value.ToString();
}
