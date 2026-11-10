using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using AiToDu.Platform.Shared.Domain.Model.Aggregates;
using AiToDu.Platform.Shared.Domain.Repositories;
using AiToDu.Platform.Shared.Application.Exceptions;
using AiToDu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using AiToDu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Models;
namespace AiToDu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public sealed class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public async Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        var aggregates = context.ChangeTracker.Entries<AggregateRoot>().Select(x => x.Entity).ToList();
        foreach (var fact in aggregates.SelectMany(x => x.DomainEvents))
            context.OutboxMessages.Add(new OutboxMessage
            {
                Id = fact.EventId,
                EventType = fact.GetType().Name,
                Payload = JsonSerializer.Serialize(fact, fact.GetType()),
                OccurredAt = fact.OccurredAt.ToUniversalTime()
            });
        try { await context.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException e) { throw new PersistenceConflictException("VersionConflict", e); }
        catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: "23505" } pg)
        { throw new PersistenceConflictException(pg.ConstraintName == "ux_inventory_items_organization_sku" ? "DuplicateSku" : "PersistenceConflict", e); }
        foreach (var aggregate in aggregates) aggregate.ClearDomainEvents();
    }
}
