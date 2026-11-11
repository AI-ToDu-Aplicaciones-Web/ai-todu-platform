using AiToDu.Platform.Sales.Domain.Model;

namespace AiToDu.Platform.Sales.Application;

public interface ISalesDirectory
{
    Task<List<Sale>> History(DateTime? startUtc, DateTime? endUtc, CancellationToken ct);
    Task<List<Customer>> CustomersMatching(string? search, CancellationToken ct);
    Task<Customer> RegisterCustomer(string name, CancellationToken ct);
    Task<Sale?> FindSale(Guid id, CancellationToken ct);
}
