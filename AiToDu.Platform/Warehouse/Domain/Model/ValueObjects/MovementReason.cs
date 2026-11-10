using AiToDu.Platform.Warehouse.Domain.Model.Errors;
namespace AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;

public sealed record MovementReason
{
    public string Value { get; }
    public MovementReason(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length > 500)
            throw new WarehouseDomainException(WarehouseErrorCode.InvalidMovementReason);
        Value = value.Trim();
    }
    public override string ToString() => Value;
}
