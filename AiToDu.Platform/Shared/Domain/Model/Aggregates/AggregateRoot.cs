using AiToDu.Platform.Shared.Domain.Model.Events;

namespace AiToDu.Platform.Shared.Domain.Model.Aggregates;

/// <summary>
///     Base class for aggregate roots in Domain-Driven Design.
///     Encapsulates domain event collection and management.
/// </summary>
public abstract class AggregateRoot : IAggregateRoot
{
    private readonly List<Func<IEvent>> _domainEventFactories = [];

    /// <inheritdoc />
    public IReadOnlyCollection<IEvent> DomainEvents =>
        _domainEventFactories.Select(factory => factory()).ToList().AsReadOnly();

    /// <summary>
    ///     Records a domain event to be dispatched when changes are committed.
    /// </summary>
    /// <param name="domainEventFactory">Builds the domain event, evaluated when <see cref="DomainEvents" /> is read.</param>
    protected void AddDomainEvent(Func<IEvent> domainEventFactory)
    {
        _domainEventFactories.Add(domainEventFactory);
    }

    /// <inheritdoc />
    public void ClearDomainEvents()
    {
        _domainEventFactories.Clear();
    }
}
