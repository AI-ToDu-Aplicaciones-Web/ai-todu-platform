namespace AiToDu.Platform.Shared.Domain.Model.Events;

/// <summary>A fact raised by a domain aggregate; independent of delivery frameworks.</summary>
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTimeOffset OccurredAt { get; }
}
