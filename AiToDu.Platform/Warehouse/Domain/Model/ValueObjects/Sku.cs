using AiToDu.Platform.Warehouse.Domain.Model.Errors;
namespace AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;

public sealed record Sku
{
    public string Value { get; }
    public Sku(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length > 64)
            throw new WarehouseDomainException(WarehouseErrorCode.InvalidSku);
        Value = value.Trim().ToUpperInvariant();
    }
    public override string ToString() => Value;
}
