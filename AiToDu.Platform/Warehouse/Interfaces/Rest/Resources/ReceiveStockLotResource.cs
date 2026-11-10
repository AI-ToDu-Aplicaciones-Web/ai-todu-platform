using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
namespace AiToDu.Platform.Warehouse.Interfaces.Rest.Resources;

public sealed record ReceiveStockLotResource(Guid OperationId, long ExpectedVersion, string LotCode, string Location, DateOnly? ExpiresOn, decimal Quantity);
