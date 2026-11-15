using System;
using System.Collections.Generic;
using System.Linq;
using AiToDu.Platform.Shared.Domain.Model.Events;

namespace AiToDu.Platform.Shared.Domain.Model.Aggregates;

public abstract class AggregateRoot : IAggregateRoot
{
    private readonly List<Func<IEvent>> _domainEventFactories = [];

    public IReadOnlyCollection<IEvent> DomainEvents =>
        _domainEventFactories.Select(factory => factory()).ToList().AsReadOnly();

    protected void AddDomainEvent(Func<IEvent> domainEventFactory) =>
        _domainEventFactories.Add(domainEventFactory);

    protected void AddDomainEvent(IEvent domainEvent) =>
        _domainEventFactories.Add(() => domainEvent);

    public void ClearDomainEvents() => _domainEventFactories.Clear();
}