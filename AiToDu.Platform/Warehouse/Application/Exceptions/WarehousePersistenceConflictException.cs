using AiToDu.Platform.Warehouse.Domain.Model.Errors;
namespace AiToDu.Platform.Warehouse.Application.Exceptions;
public sealed class WarehousePersistenceConflictException(WarehouseErrorCode code, Exception inner) : Exception(code.ToString(), inner)
{ public WarehouseErrorCode Code { get; } = code; }
