using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
namespace AiToDu.Platform.Warehouse.Interfaces.Rest.Resources;

public sealed record RecordInventoryMovementResource(Guid OperationId, long ExpectedVersion, Guid LotId, MovementType Type, decimal Quantity, string Reason);
