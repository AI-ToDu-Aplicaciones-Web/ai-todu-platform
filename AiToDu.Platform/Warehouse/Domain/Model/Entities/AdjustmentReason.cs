namespace AiToDu.Platform.Warehouse.Domain.Model.Entities;

public sealed class AdjustmentReason
{
    public Guid Id { get; private set; }
    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = "";
    private AdjustmentReason() { }
    public AdjustmentReason(Guid organizationId, string name)
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        Name = new ValueObjects.MovementReason(name).Value;
    }
}
