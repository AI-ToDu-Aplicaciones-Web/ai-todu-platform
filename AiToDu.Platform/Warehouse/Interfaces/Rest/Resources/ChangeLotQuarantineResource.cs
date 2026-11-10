using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
namespace AiToDu.Platform.Warehouse.Interfaces.Rest.Resources;

public sealed record ChangeLotQuarantineResource(Guid OperationId, long ExpectedVersion, bool IsQuarantined, string Reason);
