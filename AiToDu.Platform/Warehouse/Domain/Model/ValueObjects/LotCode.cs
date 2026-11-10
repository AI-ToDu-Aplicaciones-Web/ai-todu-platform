using AiToDu.Platform.Warehouse.Domain.Model.Errors;
namespace AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;

public sealed record LotCode
{
    public string Value { get; }
    public LotCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length > 64)
            throw new WarehouseDomainException(WarehouseErrorCode.InvalidLotCode);
        Value = value.Trim().ToUpperInvariant();
    }
    public override string ToString() => Value;
}
