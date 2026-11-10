namespace AiToDu.Platform.Warehouse.Domain.Model.Errors;

/// <summary>Business rejection expressed without HTTP or persistence dependencies.</summary>
public sealed class WarehouseDomainException(WarehouseErrorCode code, bool conflict = false) : Exception(code.ToString())
{
    public WarehouseErrorCode ErrorCode { get; } = code;
    public string Code => ErrorCode.ToString();
    public bool IsConflict { get; } = conflict;
}
