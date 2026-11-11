using System.Data;
using AiToDu.Platform.Sales.Application;
using AiToDu.Platform.Sales.Domain.Model;
using AiToDu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using AiToDu.Platform.Warehouse.Application.OutboundServices;
using AiToDu.Platform.Warehouse.Application.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace AiToDu.Platform.Sales.Infrastructure.Persistence;

public sealed class SalesRepository(AppDbContext db, ICurrentWarehouseActor actor) : ISalesRepository, ISalesDirectory
{
    public Guid Organization => actor.OrganizationId.Value;
    public Guid Actor => actor.ActorId.Value;
    public IQueryable<Customer> Customers => db.Set<Customer>().Where(x => x.OrganizationId == Organization);
    public IQueryable<Sale> Sales => db.Set<Sale>().Where(x => x.OrganizationId == Organization).AsSplitQuery();
    public Task<List<Sale>> History(DateTime? startUtc, DateTime? endUtc, CancellationToken ct)
    {
        var query = Sales.AsNoTracking();
        if (startUtc.HasValue) query = query.Where(x => x.CreatedAt >= startUtc.Value);
        if (endUtc.HasValue) query = query.Where(x => x.CreatedAt < endUtc.Value);
        return query.OrderByDescending(x => x.CreatedAt).ToListAsync(ct);
    }
    public Task<List<Customer>> CustomersMatching(string? search, CancellationToken ct)
    {
        var query = Customers.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.Name.Contains(search.Trim()));
        return query.OrderBy(x => x.Name).ToListAsync(ct);
    }
    public Task<Customer?> FindCustomer(Guid id, CancellationToken ct) => Customers.SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<Sale?> FindSale(Guid id, CancellationToken ct) => Sales.SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<Sale?> FindOperation(Guid id, CancellationToken ct) => Sales.SingleOrDefaultAsync(x => x.OperationId == id, ct);
    public void Add(Sale sale) => db.Add(sale);
    public async Task<Customer> RegisterCustomer(string name, CancellationToken ct)
    {
        var customer = new Customer(Organization, name);
        return await Atomic(async () => {
            var existing = await Customers.SingleOrDefaultAsync(x => x.Name == customer.Name, ct);
            if (existing != null) return existing;
            db.Add(customer);
            return customer;
        }, ct);
    }

    public async Task<T> Atomic<T>(Func<Task<T>> operation, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        try
        {
            var result = await operation();
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return result;
        }
        catch (WarehousePersistenceConflictException) { throw new SalesError("Concurrent operation. Retry with the same reference."); }
        catch (DbUpdateConcurrencyException) { throw new SalesError("Concurrent operation. Reload and retry."); }
    }
}
