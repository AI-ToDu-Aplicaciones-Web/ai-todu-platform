using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
namespace AiToDu.Platform.Warehouse.Interfaces.Rest.Resources;

public sealed record RelocateStockLotResource(Guid OperationId, long ExpectedVersion, string Location, string Reason);
