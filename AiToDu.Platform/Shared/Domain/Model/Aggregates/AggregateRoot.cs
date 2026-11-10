using AiToDu.Platform.Shared.Domain.Model.Events;
namespace AiToDu.Platform.Shared.Domain.Model.Aggregates;

/// <summary>Collects domain facts until the enclosing unit of work commits.</summary>
public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
