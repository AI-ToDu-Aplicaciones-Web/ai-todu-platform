namespace AiToDu.Platform.Sales.Domain.Model;

public sealed class Customer
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = "";
    private Customer() { }
    public Customer(Guid organization, string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 160) throw new ArgumentException("Invalid customer name.");
        OrganizationId = organization;
        Name = name.Trim();
    }
}
