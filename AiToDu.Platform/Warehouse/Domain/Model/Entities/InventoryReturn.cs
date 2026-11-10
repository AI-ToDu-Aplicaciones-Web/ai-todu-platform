using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
using AiToDu.Platform.Warehouse.Domain.Model.Errors;
using AiToDu.Platform.Shared.Domain.Model.ValueObjects;

namespace AiToDu.Platform.Warehouse.Domain.Model.Entities;

public sealed class InventoryReturn
{
    private InventoryReturn() { }
    public Guid Id { get; private set; }
    public Guid OperationId { get; private set; }
    public StockLotId SourceLotId { get; private set; }
    public StockLotId? DestinationLotId { get; private set; }
    public decimal Quantity { get; private set; }
    public ReturnDisposition Disposition { get; private set; }
    public string Reference { get; private set; } = "";
    public string Reason { get; private set; } = "";
    public ActorId ActorId { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    internal InventoryReturn(Guid operationId, StockLotId source, StockLotId? destination, decimal quantity, ReturnDisposition disposition, string reference, string reason, ActorId actorId, DateTimeOffset now)
    { Id = Guid.NewGuid(); OperationId = operationId; SourceLotId = source; DestinationLotId = destination; Quantity = quantity; Disposition = disposition; Reference = reference; Reason = reason; ActorId = actorId; OccurredAt = now.ToUniversalTime(); }
}
