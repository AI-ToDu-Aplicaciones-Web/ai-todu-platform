using AiToDu.Platform.Warehouse.Domain.Model.Errors;
namespace AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;

public sealed record StorageLocation
{
    public string Value { get; }
    public StorageLocation(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length > 120)
            throw new WarehouseDomainException(WarehouseErrorCode.InvalidStorageLocation);
        Value = value.Trim();
    }
    public override string ToString() => Value;
}
