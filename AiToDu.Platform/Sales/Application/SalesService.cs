using System.Security.Cryptography;
using System.Text.Json;
using AiToDu.Platform.Sales.Domain.Model;
using AiToDu.Platform.Warehouse.Interfaces.Acl;

namespace AiToDu.Platform.Sales.Application;

public sealed record CartItem(int ItemId, decimal Quantity);
public sealed record Checkout(Guid OperationId, Guid CustomerId, List<CartItem> Items, bool? PaymentAccepted, string PaymentReference);
public sealed record ReturnRequest(Guid OperationId, Guid LineId, decimal Quantity, string Disposition, string Reason);
public sealed class SalesError(string message, int status = 409) : Exception(message) { public int Status => status; }
public interface ISalesRepository
{
    Guid Organization { get; }
    Guid Actor { get; }
    Task<Customer?> FindCustomer(Guid id, CancellationToken ct);
    Task<Sale?> FindSale(Guid id, CancellationToken ct);
    Task<Sale?> FindOperation(Guid id, CancellationToken ct);
    Task<T> Atomic<T>(Func<Task<T>> operation, CancellationToken ct);
    void Add(Sale sale);
}

public sealed class SalesService(ISalesRepository repository, IWarehouseContextFacade warehouse)
{
    public async Task<Sale> Checkout(Checkout request, CancellationToken ct)
    {
        if (request.OperationId == Guid.Empty || request.CustomerId == Guid.Empty || request.PaymentAccepted is null ||
            string.IsNullOrWhiteSpace(request.PaymentReference) || request.PaymentReference.Length > 160 ||
            request.Items is not { Count: > 0 and <= 100 } || request.Items.Any(x => x is null || x.ItemId <= 0 ||
                x.Quantity <= 0 || x.Quantity > 999999999 || decimal.Round(x.Quantity, 3) != x.Quantity) ||
            request.Items.Select(x => x.ItemId).Distinct().Count() != request.Items.Count)
            throw new SalesError("Invalid cart or payment.", 400);

        var hash = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(request)));
        return await repository.Atomic(async () => {
            var previous = await repository.FindOperation(request.OperationId, ct);
            if (previous != null)
                return previous.RequestHash == hash ? previous : throw new SalesError("Operation already used with different data.");

            var customer = await repository.FindCustomer(request.CustomerId, ct) ?? throw new SalesError("Unknown customer.", 404);
            var sale = new Sale(repository.Organization, repository.Actor, request.OperationId, customer, hash);
            foreach (var row in request.Items.OrderBy(x => x.ItemId))
            {
                var quote = await warehouse.Quote(row.ItemId, ct) ?? throw new SalesError("Unknown product.", 404);
                var remaining = row.Quantity;
                var version = quote.Version;
                foreach (var lot in quote.Lots)
                {
                    var units = Math.Min(remaining, lot.Quantity);
                    sale.AddLine(row.ItemId, lot.Id, quote.Sku, quote.Name, quote.UnitPrice, units);
                    if (request.PaymentAccepted.Value && !await warehouse.IssueForSale(row.ItemId, lot.Id, Guid.NewGuid(),
                        version++, units, sale.Id.ToString(), ct)) throw new SalesError("Stock changed. Review the cart.");
                    remaining -= units;
                    if (remaining == 0) break;
                }
                if (remaining > 0) throw new SalesError($"Insufficient stock for {quote.Sku}.");
            }
            sale.RecordPayment(request.PaymentAccepted.Value, request.PaymentReference.Trim());
            repository.Add(sale);
            return sale;
        }, ct);
    }

    public Task<Sale> Cancel(Guid saleId, CancellationToken ct) => repository.Atomic(async () => {
        var sale = await repository.FindSale(saleId, ct) ?? throw new SalesError("Unknown sale.", 404);
        if (!sale.Cancel()) throw new SalesError("Return paid merchandise using its receipt.");
        return sale;
    }, ct);

    public Task<Sale> Pay(Guid saleId, string reference, CancellationToken ct) => repository.Atomic(async () => {
        if (string.IsNullOrWhiteSpace(reference) || reference.Length > 160) throw new SalesError("Invalid payment reference.", 400);
        var sale = await repository.FindSale(saleId, ct) ?? throw new SalesError("Unknown sale.", 404);
        if (sale.Status == "Paid") return sale.PaymentReference == reference.Trim() ? sale : throw new SalesError("Payment already recorded.");
        if (sale.Status != "Pending") throw new SalesError("Cancelled sale.");
        foreach (var group in sale.Lines.GroupBy(x => x.ItemId).OrderBy(x => x.Key))
        {
            var quote = await warehouse.Quote(group.Key, ct) ?? throw new SalesError("Product unavailable.");
            var version = quote.Version;
            foreach (var line in group)
                if (!await warehouse.IssueForSale(line.ItemId, line.LotId, Guid.NewGuid(), version++, line.Quantity, sale.Id.ToString(), ct))
                    throw new SalesError("Stock is unavailable.");
        }
        sale.RecordPayment(true, reference.Trim());
        return sale;
    }, ct);

    public async Task ReturnByReceipt(int itemId, Guid lot, Guid operation, string reference, decimal quantity, string disposition, string reason, CancellationToken ct)
    {
        if (!Guid.TryParse(reference?.Replace("AT-", "", StringComparison.OrdinalIgnoreCase), out var id))
            throw new SalesError("Enter a valid sale receipt.", 400);
        var sale = await repository.FindSale(id, ct) ?? throw new SalesError("Unknown sale.", 404);
        var line = sale.Lines.SingleOrDefault(x => x.ItemId == itemId && x.LotId == lot) ?? throw new SalesError("Lot is not part of this sale.");
        await Return(id, new(operation, line.Id, quantity, disposition, reason), ct);
    }

    public Task<Sale> Return(Guid saleId, ReturnRequest request, CancellationToken ct) => repository.Atomic(async () => {
        if (request.OperationId == Guid.Empty || string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Length > 160 ||
            request.Disposition is not ("Restock" or "Quarantine" or "Rejected")) throw new SalesError("Invalid return.", 400);
        var sale = await repository.FindSale(saleId, ct) ?? throw new SalesError("Unknown sale.", 404);
        var previous = sale.Returns.SingleOrDefault(x => x.OperationId == request.OperationId);
        if (previous != null)
        {
            if (previous.LineId != request.LineId || previous.Quantity != request.Quantity || previous.Disposition != request.Disposition ||
                previous.Reason != request.Reason) throw new SalesError("Return reference already used.");
            return sale;
        }
        var line = sale.Lines.SingleOrDefault(x => x.Id == request.LineId);
        if (sale.Status != "Paid" || line is null || !line.TryReturn(request.Quantity)) throw new SalesError("Invalid return quantity or sale.");
        if (!await warehouse.AcceptSaleReturn(line.ItemId, line.LotId, request.OperationId, request.Quantity,
            request.Disposition, sale.Id.ToString(), request.Reason, ct)) throw new SalesError("Return could not be recorded.");
        sale.RegisterReturn(new SaleReturn(request.OperationId, line.Id, request.Quantity, request.Disposition, request.Reason));
        return sale;
    }, ct);
}
