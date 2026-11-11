namespace AiToDu.Platform.Sales.Domain.Model;

public sealed class Sale
{
    public Guid Id { get; private set; }
    public Guid OrganizationId { get; private set; }
    public Guid OperationId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string CustomerName { get; private set; } = "";
    public string RequestHash { get; private set; } = "";
    public DateTime CreatedAt { get; private set; }
    public Guid ActorId { get; private set; }
    public string Status { get; private set; } = "Pending";
    public string PaymentReference { get; private set; } = "";
    public string? Receipt { get; private set; }
    public decimal Total { get; private set; }
    private readonly List<SaleLine> _lines = [];
    public IReadOnlyCollection<SaleLine> Lines => _lines.AsReadOnly();

    private readonly List<SaleReturn> _returns = [];
    public IReadOnlyCollection<SaleReturn> Returns => _returns.AsReadOnly();
    public void RegisterReturn(SaleReturn entry) => _returns.Add(entry);

    private Sale() { }
    public Sale(Guid org, Guid actor, Guid operation, Customer customer, string hash)
    {
        Id = Guid.NewGuid();
        OperationId = operation;
        OrganizationId = org;
        ActorId = actor;
        CustomerId = customer.Id;
        CustomerName = customer.Name;
        RequestHash = hash;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddLine(int item, Guid lot, string sku, string name, decimal price, decimal quantity)
    {
        if (Status != "Pending" || quantity <= 0 || decimal.Round(quantity, 3) != quantity || price < 0)
            throw new InvalidOperationException("Invalid sale line.");
        var line = new SaleLine(item, lot, sku, name, price, quantity);
        _lines.Add(line);
        Total += line.Amount;
    }

    public void RecordPayment(bool accepted, string reference)
    {
        if (_lines.Count == 0 || Status == "Paid") throw new InvalidOperationException("Invalid payment state.");
        PaymentReference = reference;
        if (!accepted) return;
        Status = "Paid";
        Receipt = $"AT-{Id:N}";
    }
}

public sealed record SaleReturn(Guid OperationId, Guid LineId, decimal Quantity, string Disposition, string Reason);

public sealed class SaleLine
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public int ItemId { get; private set; }
    public Guid LotId { get; private set; }
    public string Sku { get; private set; } = "";
    public string Name { get; private set; } = "";
    public decimal UnitPrice { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal Returned { get; private set; }
    public decimal Amount { get; private set; }
    private SaleLine() { }
    internal SaleLine(int item, Guid lot, string sku, string name, decimal price, decimal quantity)
    {
        ItemId = item; LotId = lot; Sku = sku; Name = name; UnitPrice = price; Quantity = quantity;
        Amount = decimal.Round(price * quantity, 2, MidpointRounding.AwayFromZero);
    }

    public bool TryReturn(decimal quantity)
    {
        if (quantity <= 0 || decimal.Round(quantity, 3) != quantity || quantity > Quantity - Returned) return false;
        Returned += quantity;
        return true;
    }
}
