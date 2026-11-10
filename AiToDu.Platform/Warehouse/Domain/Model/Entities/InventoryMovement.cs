using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
using AiToDu.Platform.Warehouse.Domain.Model.Errors;

namespace AiToDu.Platform.Warehouse.Domain.Model.Entities;

public sealed class InventoryMovement
{
    private InventoryMovement() { }
    public Guid Id { get; private set; }
    public Guid OperationId { get; private set; }
    public StockLotId LotId { get; private set; }
    public MovementType Type { get; private set; }
    public decimal Change { get; private set; }
    public decimal Before { get; private set; }
    public decimal After { get; private set; }
    public string Reason { get; private set; } = "";
    public ActorId ActorId { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    internal InventoryMovement(Guid operationId, StockLotId lotId, MovementType type, decimal before, decimal after, string reason, ActorId actorId, DateTimeOffset now)
    { Id = Guid.NewGuid(); OperationId = operationId; LotId = lotId; Type = type; Before = before; After = after; Change = after - before; Reason = reason; ActorId = actorId; OccurredAt = now.ToUniversalTime(); }
}
