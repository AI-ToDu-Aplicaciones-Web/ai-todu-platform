using AiToDu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.ValueConverters;
using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
using AiToDu.Platform.Warehouse.Domain.Model.Errors;
using Microsoft.EntityFrameworkCore;
using AiToDu.Platform.Warehouse.Domain.Model.Aggregates;
using AiToDu.Platform.Warehouse.Domain.Model.Entities;
namespace AiToDu.Platform.Warehouse.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyWarehouseConfiguration(this ModelBuilder builder)
    {
        var item = builder.Entity<InventoryItem>();
        item.ToTable("inventory_items", "warehouse"); item.HasKey(x => x.Id);
        item.Property(x => x.Id).HasConversion(new StronglyTypedIdConverter<InventoryItemId, int>(x => new InventoryItemId(x))).ValueGeneratedOnAdd();
        item.Property(x => x.OrganizationId).HasConversion(x => x.Value, x => new OrganizationId(x));
        item.Property(x => x.Sku).HasConversion(x => x.Value, x => new Sku(x)).HasMaxLength(64);
        item.Property(x => x.Name).HasMaxLength(160); item.Property(x => x.Category).HasMaxLength(100);
        item.Property(x => x.MinimumStock).HasConversion(x => x.Value, x => new StockQuantity(x)).HasPrecision(18, 3);
        item.Property(x => x.UnitPrice).HasPrecision(18, 2);
        item.Property(x => x.Version).IsConcurrencyToken();
        item.HasIndex(x => new { x.OrganizationId, x.Sku }).IsUnique().HasDatabaseName("ux_inventory_items_organization_sku");
        item.Ignore(x => x.DomainEvents); item.Ignore(x => x.TotalQuantity);

        item.OwnsMany(x => x.Lots, lot =>
        {
            lot.ToTable("stock_lots", "warehouse");
            lot.WithOwner().HasForeignKey("inventory_item_id");
            lot.Property<InventoryItemId>("inventory_item_id").HasConversion(new StronglyTypedIdConverter<InventoryItemId, int>(x => new InventoryItemId(x)));
            lot.HasKey("inventory_item_id", nameof(StockLot.Id));
            lot.Property(x => x.Id).ValueGeneratedNever();
            lot.Property(x => x.Id).HasConversion(new StronglyTypedIdConverter<StockLotId, Guid>(x => new StockLotId(x)));
            lot.Property(x => x.Code).HasConversion(x => x.Value, x => new LotCode(x)).HasMaxLength(64);
            lot.Property(x => x.Location).HasConversion(x => x.Value, x => new StorageLocation(x)).HasMaxLength(120);
            lot.Property(x => x.Quantity).HasConversion(x => x.Value, x => new StockQuantity(x)).HasPrecision(18, 3);
            lot.HasIndex("inventory_item_id", nameof(StockLot.Code), nameof(StockLot.Location)).IsUnique();
        });
        item.Navigation(x => x.Lots).UsePropertyAccessMode(PropertyAccessMode.Field);

        item.OwnsMany(x => x.Movements, movement =>
        {
            movement.ToTable("inventory_movements", "warehouse");
            movement.WithOwner().HasForeignKey("inventory_item_id");
            movement.Property<InventoryItemId>("inventory_item_id").HasConversion(new StronglyTypedIdConverter<InventoryItemId, int>(x => new InventoryItemId(x)));
            movement.HasKey("inventory_item_id", nameof(InventoryMovement.Id));
            movement.Property(x => x.Id).ValueGeneratedNever();
            movement.Property(x => x.LotId).HasConversion(new StronglyTypedIdConverter<StockLotId, Guid>(x => new StockLotId(x)));
            movement.Property(x => x.ActorId).HasConversion(x => x.Value, x => new ActorId(x));
            movement.Property(x => x.Type).HasConversion<string>();
            movement.Property(x => x.Change).HasPrecision(18, 3); movement.Property(x => x.Before).HasPrecision(18, 3); movement.Property(x => x.After).HasPrecision(18, 3);
        });
        item.Navigation(x => x.Movements).UsePropertyAccessMode(PropertyAccessMode.Field);

        item.OwnsMany(x => x.Counts, count =>
        {
            count.ToTable("stock_counts", "warehouse");
            count.WithOwner().HasForeignKey("inventory_item_id");
            count.Property<InventoryItemId>("inventory_item_id").HasConversion(new StronglyTypedIdConverter<InventoryItemId, int>(x => new InventoryItemId(x)));
            count.HasKey("inventory_item_id", nameof(StockCount.Id));
            count.Property(x => x.Id).ValueGeneratedNever();
            count.Property(x => x.LotId).HasConversion(new StronglyTypedIdConverter<StockLotId, Guid>(x => new StockLotId(x)));
            count.Property(x => x.ActorId).HasConversion(x => x.Value, x => new ActorId(x));
            count.Property(x => x.ExpectedQuantity).HasPrecision(18, 3); count.Property(x => x.CountedQuantity).HasPrecision(18, 3);
        });
        item.Navigation(x => x.Counts).UsePropertyAccessMode(PropertyAccessMode.Field);

        item.OwnsMany(x => x.Returns, returned =>
        {
            returned.ToTable("inventory_returns", "warehouse");
            returned.WithOwner().HasForeignKey("inventory_item_id");
            returned.Property<InventoryItemId>("inventory_item_id").HasConversion(new StronglyTypedIdConverter<InventoryItemId, int>(x => new InventoryItemId(x)));
            returned.HasKey("inventory_item_id", nameof(InventoryReturn.Id));
            returned.Property(x => x.Id).ValueGeneratedNever();
            returned.Property(x => x.SourceLotId).HasConversion(new StronglyTypedIdConverter<StockLotId, Guid>(x => new StockLotId(x)));
            returned.Property(x => x.DestinationLotId).HasConversion(x => x!.Value.Value, x => new StockLotId(x));
            returned.Property(x => x.ActorId).HasConversion(x => x.Value, x => new ActorId(x));
            returned.Property(x => x.Disposition).HasConversion<string>();
            returned.Property(x => x.Quantity).HasPrecision(18, 3);
        });
        item.Navigation(x => x.Returns).UsePropertyAccessMode(PropertyAccessMode.Field);

        item.OwnsMany(x => x.Operations, operation =>
        {
            operation.ToTable("inventory_operations", "warehouse");
            operation.WithOwner().HasForeignKey("inventory_item_id");
            operation.Property<InventoryItemId>("inventory_item_id").HasConversion(new StronglyTypedIdConverter<InventoryItemId, int>(x => new InventoryItemId(x)));
            operation.HasKey("inventory_item_id", nameof(InventoryOperation.Id));
            operation.Property(x => x.Id).ValueGeneratedNever();

        });
        item.Navigation(x => x.Operations).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
