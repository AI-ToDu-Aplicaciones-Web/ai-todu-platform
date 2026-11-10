using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
namespace AiToDu.Platform.Warehouse.Interfaces.Rest.Resources;

public sealed record RegisterStockCountResource(Guid OperationId, long ExpectedVersion, Guid LotId, decimal ExpectedQuantity, decimal CountedQuantity, string Reason);
