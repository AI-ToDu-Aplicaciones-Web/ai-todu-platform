using AiTodu.Platform.Shared.Domain.Model.Events;

namespace AiTodu.Platform.Shared.Domain.Model.Aggregates;

public interface IAggregateRoot
{
    IReadOnlyCollection<IEvent> DomainEvents { get; }
    void ClearDomainEvents();
}

public abstract class AggregateRoot : IAggregateRoot
{
    private readonly List<Func<IEvent>> _domainEventFactories = [];

    public IReadOnlyCollection<IEvent> DomainEvents =>
        _domainEventFactories.Select(factory => factory()).ToList().AsReadOnly();

    protected void AddDomainEvent(Func<IEvent> domainEventFactory) =>
        _domainEventFactories.Add(domainEventFactory);

    public void ClearDomainEvents() => _domainEventFactories.Clear();
}