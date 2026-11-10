namespace AiToDu.Platform.Warehouse.Interfaces.Rest.Resources;

public sealed record InventoryMovementResource(Guid Id, Guid OperationId, Guid LotId, string Type, decimal Change, decimal Before, decimal After, string Reason, Guid ActorId, DateTimeOffset OccurredAt);
