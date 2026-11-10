using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
using AiToDu.Platform.Warehouse.Domain.Model.Errors;
using AiToDu.Platform.Shared.Domain.Model.ValueObjects;

namespace AiToDu.Platform.Warehouse.Domain.Model.Entities;

public sealed class StockCount
{
    private StockCount() { }
    public Guid Id { get; private set; }
    public Guid OperationId { get; private set; }
    public StockLotId LotId { get; private set; }
    public decimal ExpectedQuantity { get; private set; }
    public decimal CountedQuantity { get; private set; }
    public string Reason { get; private set; } = "";
    public ActorId ActorId { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    internal StockCount(Guid operationId, StockLotId lotId, decimal expected, decimal counted, string reason, ActorId actorId, DateTimeOffset now)
    { Id = Guid.NewGuid(); OperationId = operationId; LotId = lotId; ExpectedQuantity = expected; CountedQuantity = counted; Reason = reason; ActorId = actorId; OccurredAt = now.ToUniversalTime(); }
}
