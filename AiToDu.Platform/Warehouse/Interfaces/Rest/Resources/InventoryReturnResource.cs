namespace AiToDu.Platform.Warehouse.Interfaces.Rest.Resources;

public sealed record InventoryReturnResource(Guid Id, Guid SourceLotId, Guid? DestinationLotId, decimal Quantity, string Disposition, string Reference, string Reason, DateTimeOffset OccurredAt);
