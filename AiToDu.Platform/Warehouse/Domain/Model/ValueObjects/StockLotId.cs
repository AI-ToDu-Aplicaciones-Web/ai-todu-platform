using AiToDu.Platform.Shared.Domain.Model.ValueObjects;
namespace AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
public readonly record struct StockLotId(Guid Value) : IEntityId<Guid>
{
    public static StockLotId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}
