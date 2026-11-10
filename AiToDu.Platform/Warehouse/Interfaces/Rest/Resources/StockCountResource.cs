namespace AiToDu.Platform.Warehouse.Interfaces.Rest.Resources;

public sealed record StockCountResource(Guid Id, Guid LotId, decimal ExpectedQuantity, decimal CountedQuantity, string Reason, DateTimeOffset OccurredAt);
