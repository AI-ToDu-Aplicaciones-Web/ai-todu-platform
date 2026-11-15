namespace AiToDu.Platform.Activity.Domain.Model;

public sealed record AuditEntry(
    Guid Id,
    Guid OrganizationId,
    Guid ActorId,
    string Module,
    string Method,
    string Path,
    int StatusCode,
    DateTime OccurredAt);
public sealed record NotificationRead(Guid OrganizationId, Guid ActorId, string AlertKey, DateTime ReadAt);
public sealed record StockAlert(string Key, string Kind, string Sku, int ItemId, decimal Quantity, DateOnly? ExpiresOn, bool IsRead);
