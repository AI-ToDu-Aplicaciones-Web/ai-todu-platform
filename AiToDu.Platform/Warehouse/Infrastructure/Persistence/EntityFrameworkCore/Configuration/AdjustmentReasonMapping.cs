using AiToDu.Platform.Warehouse.Domain.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiToDu.Platform.Warehouse.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

public sealed class AdjustmentReasonMapping : IEntityTypeConfiguration<AdjustmentReason>
{
    public void Configure(EntityTypeBuilder<AdjustmentReason> e)
    {
        e.ToTable("warehouse_adjustment_reasons"); e.HasKey(x => x.Id);
        e.Property(x => x.Name).HasMaxLength(120);
        e.HasIndex(x => new {x.OrganizationId,x.Name}).IsUnique().HasDatabaseName("ux_org_adjustment_reason");
    }
}
