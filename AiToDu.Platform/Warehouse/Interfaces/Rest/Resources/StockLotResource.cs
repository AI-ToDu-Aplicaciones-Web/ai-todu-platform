namespace AiToDu.Platform.Warehouse.Interfaces.Rest.Resources;

public sealed record StockLotResource(Guid Id, string Code, string Location, DateOnly? ExpiresOn, decimal Quantity, bool IsQuarantined, bool IsExpired);
