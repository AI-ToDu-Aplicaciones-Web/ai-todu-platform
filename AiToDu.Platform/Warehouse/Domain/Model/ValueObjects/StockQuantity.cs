using AiToDu.Platform.Warehouse.Domain.Model.Errors;
namespace AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;

public readonly record struct StockQuantity
{
    public decimal Value { get; }
    public StockQuantity(decimal value)
    {
        if (value < 0 || value > 999999999999m || decimal.Round(value, 3) != value)
            throw new WarehouseDomainException(WarehouseErrorCode.InvalidQuantity);
        Value = value;
    }
    public static StockQuantity Positive(decimal value)
    {
        var quantity = new StockQuantity(value);
        if (quantity.Value == 0) throw new WarehouseDomainException(WarehouseErrorCode.PositiveQuantityRequired);
        return quantity;
    }
}
