using AiToDu.Platform.Shared.Application.Internal.EventHandlers;
using AiToDu.Platform.Warehouse.Domain.Model.Events;
namespace AiToDu.Platform.Warehouse.Application.Internal.EventHandlers;
public sealed class InventoryChangedEventHandler(ILogger<InventoryChangedEventHandler> logger) : IEventHandler<InventoryChangedEvent>
{
    public Task Handle(InventoryChangedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Inventory {InventoryId}: {ChangeType}; operation {OperationId}",domainEvent.InventoryItemId.Value,domainEvent.ChangeType,domainEvent.OperationId);
        return Task.CompletedTask;
    }
}
