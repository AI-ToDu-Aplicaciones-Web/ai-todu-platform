using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
using AiToDu.Platform.Warehouse.Domain.Model.Errors;

namespace AiToDu.Platform.Warehouse.Domain.Model.Entities;

public sealed class InventoryOperation
{
    private InventoryOperation() { }
    public Guid Id { get; private set; }
    public string Signature { get; private set; } = "";
    internal InventoryOperation(Guid id, string signature) { Id = id; Signature = signature; }
}
