using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
using AiToDu.Platform.Warehouse.Domain.Model.Errors;
using AiToDu.Platform.Shared.Domain.Model.ValueObjects;
using AiToDu.Platform.Shared.Domain.Model.Events;
namespace AiToDu.Platform.Warehouse.Domain.Model.Events;

public sealed record InventoryChangedEvent(Guid EventId, OrganizationId OrganizationId,
    InventoryItemId InventoryItemId, Guid OperationId, string ChangeType, ActorId ActorId,
    DateTimeOffset OccurredAt) : IDomainEvent;
