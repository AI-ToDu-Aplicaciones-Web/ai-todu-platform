using AiToDu.Platform.Shared.Domain.Model.Events;
using Cortex.Mediator.Notifications;

namespace AiToDu.Platform.Shared.Application.Internal.EventHandlers;

public interface IEventHandler<in TEvent> : INotificationHandler<TEvent> where TEvent : IEvent
{
}
