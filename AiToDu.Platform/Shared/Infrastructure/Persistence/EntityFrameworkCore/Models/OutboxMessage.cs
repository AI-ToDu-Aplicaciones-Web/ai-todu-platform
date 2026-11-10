namespace AiToDu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Models;

/// <summary>Transactional integration record. Delivery is owned by a future outbox dispatcher.</summary>
public sealed class OutboxMessage
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = "";
    public string Payload { get; set; } = "";
    public DateTimeOffset OccurredAt { get; set; }
}
