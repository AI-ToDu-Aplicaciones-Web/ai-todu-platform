using System.Collections.Generic;
using AiToDu.Platform.Shared.Domain.Model.Events;

namespace AiToDu.Platform.Shared.Domain.Model.Aggregates;

public interface IAggregateRoot
{
    IReadOnlyCollection<IEvent> DomainEvents { get; }
    void ClearDomainEvents();
}