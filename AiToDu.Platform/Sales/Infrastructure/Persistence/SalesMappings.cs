using AiToDu.Platform.Sales.Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiToDu.Platform.Sales.Infrastructure.Persistence;

public sealed class SaleMapping : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> sale)
    {
        sale.ToTable("sales_orders");
        sale.HasKey(x => x.Id);
        sale.HasIndex(x => new { x.OrganizationId, x.OperationId }).IsUnique().HasDatabaseName("ux_sale_operation");
        sale.Property(x => x.Total).HasPrecision(18, 2);
        sale.OwnsMany(x => x.Lines, line => {
            line.ToTable("sales_lines");
            line.WithOwner().HasForeignKey("sale_id").HasConstraintName("fk_line_sale");
            line.HasKey(x => x.Id);
            line.Property(x => x.Id).ValueGeneratedNever();
            line.Property(x => x.UnitPrice).HasPrecision(18, 2);
            line.Property(x => x.Amount).HasPrecision(18, 2);
            line.Property(x => x.Quantity).HasPrecision(18, 3);
            line.Property(x => x.Returned).HasPrecision(18, 3).IsConcurrencyToken();
        });
        sale.OwnsMany(x => x.Returns, entry => {
            entry.ToTable("sales_returns");
            entry.WithOwner().HasForeignKey("sale_id").HasConstraintName("fk_return_sale");
            entry.HasKey("sale_id", nameof(SaleReturn.OperationId));
            entry.Property(x => x.Quantity).HasPrecision(18, 3);
        });
        sale.Navigation(x => x.Returns).UsePropertyAccessMode(PropertyAccessMode.Field);
        sale.Navigation(x => x.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class CustomerMapping : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> customer)
    {
        customer.ToTable("sales_customers");
        customer.HasKey(x => x.Id);
        customer.Property(x => x.Name).HasMaxLength(160);
        customer.HasIndex(x => new { x.OrganizationId, x.Name }).IsUnique().HasDatabaseName("ux_customer_name");
    }
}
