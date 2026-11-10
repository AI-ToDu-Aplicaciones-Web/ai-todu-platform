using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
namespace AiToDu.Platform.Warehouse.Application.OutboundServices;

/// <summary>Identity port supplied by the host/IAM adapter; never populated from request bodies.</summary>
public interface ICurrentWarehouseActor
{
    OrganizationId OrganizationId { get; }
    ActorId ActorId { get; }
    bool CanManageInventory { get; }
    bool CanRecordSales { get; }
}
