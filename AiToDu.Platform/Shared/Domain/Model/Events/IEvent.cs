using Cortex.Mediator.Notifications;

namespace AiToDu.Platform.Shared.Domain.Model.Events
{
    public interface IEvent : INotification
    {
    }
}

namespace AiTodu.Platform.Shared.Domain.Model.Events
{
    public interface IEvent : AiToDu.Platform.Shared.Domain.Model.Events.IEvent
    {
    }
}