using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
namespace AiToDu.Platform.Warehouse.Interfaces.Rest.Resources;

public sealed record ClassifyInventoryReturnResource(Guid OperationId, long ExpectedVersion, Guid SourceLotId, decimal Quantity, ReturnDisposition Disposition, string Reference, string Reason);
