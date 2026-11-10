using AiToDu.Platform.Shared.Domain.Model.ValueObjects;
using AiToDu.Platform.Warehouse.Domain.Model.Aggregates;
using AiToDu.Platform.Warehouse.Domain.Model.Errors;
using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
using Xunit;

namespace AiToDu.Platform.Tests;

public sealed class WarehouseDomainTests
{
    private static readonly ActorId Actor = new(Guid.NewGuid());
    private static readonly DateTimeOffset Now = new(2026, 11, 10, 12, 0, 0, TimeSpan.FromHours(-5));
    private static InventoryItem Item(decimal stock = 20)
    {
        var item = new InventoryItem(InventoryItemId.New(), new OrganizationId(Guid.NewGuid()), new Sku(" sku-1 "), "Rice", "Food", new StockQuantity(5), 12.50m, Actor, Now);
        if (stock > 0) item.ReceiveLot(Guid.NewGuid(), item.Version, new LotCode("lot-1"), new StorageLocation("A-1"), new DateOnly(2026, 12, 31), new StockQuantity(stock), Actor, Now);
        return item;
    }
    [Fact] public void SkuIsNormalizedAndComparedByValue() => Assert.Equal(new Sku("sku-1"), new Sku(" SKU-1 "));
    [Theory]
    [InlineData(-1)]
    [InlineData(1.0001)]
    public void InvalidQuantityIsRejected(decimal value) => Assert.Throws<WarehouseDomainException>(() => new StockQuantity(value));
    [Fact] public void PositiveQuantityRejectsZero() => Assert.Throws<WarehouseDomainException>(() => StockQuantity.Positive(0));
    [Fact] public void DefaultIdentifierCannotCreateAnAggregate() => Assert.Throws<WarehouseDomainException>(() => new InventoryItem(default, new OrganizationId(Guid.NewGuid()), new Sku("A"), "A", "A", new StockQuantity(0), 0, Actor, Now));
    [Fact]
    public void ReceiptAndIssueProduceAnAuditTrail()
    {
        var item = Item(); var lot = item.Lots.Single();
        item.ReceiveLot(Guid.NewGuid(), item.Version, lot.Code, lot.Location, lot.ExpiresOn, new StockQuantity(10), Actor, Now);
        item.RecordMovement(Guid.NewGuid(), item.Version, lot.Id, MovementType.Issue, 4, new MovementReason("SALE-1"), Actor, Now);
        Assert.Equal(26, item.TotalQuantity); Assert.Equal(3, item.Movements.Count);
        Assert.Equal(-4, item.Movements.Last().Change); Assert.Equal(Actor, item.Movements.Last().ActorId);
    }
    [Fact]
    public void InsufficientStockDoesNotChangeState()
    {
        var item = Item(); var version = item.Version;
        var e = Assert.Throws<WarehouseDomainException>(() => item.RecordMovement(Guid.NewGuid(), version, item.Lots.Single().Id, MovementType.Issue, 21, new MovementReason("Sale"), Actor, Now));
        Assert.Equal("InsufficientStock", e.Code); Assert.Equal(version, item.Version); Assert.Equal(20, item.TotalQuantity); Assert.Single(item.Movements);
    }
    [Fact]
    public void QuarantineExcludesAvailableStockButPreservesPhysicalStock()
    {
        var item = Item(); var lot = item.Lots.Single();
        item.SetQuarantine(Guid.NewGuid(), item.Version, lot.Id, true, new MovementReason("Inspection"), Actor, Now);
        Assert.Equal(20, item.TotalQuantity); Assert.Equal(0, item.AvailableQuantityOn(new DateOnly(2026, 11, 10)));
        Assert.Throws<WarehouseDomainException>(() => item.RecordMovement(Guid.NewGuid(), item.Version, lot.Id, MovementType.Issue, 1, new MovementReason("Sale"), Actor, Now));
    }
    [Fact]
    public void ExpirationUsesTheBusinessCalendarDate()
    {
        var item = Item(0); var late = new DateTimeOffset(2026, 11, 10, 23, 30, 0, TimeSpan.FromHours(-5));
        item.ReceiveLot(Guid.NewGuid(), item.Version, new LotCode("TODAY"), new StorageLocation("A"), new DateOnly(2026, 11, 10), new StockQuantity(2), Actor, late);
        item.RecordMovement(Guid.NewGuid(), item.Version, item.Lots.Single().Id, MovementType.Issue, 1, new MovementReason("Sale"), Actor, late);
        Assert.Equal(1, item.AvailableQuantityOn(new DateOnly(2026, 11, 10)));
        Assert.Equal(0, item.AvailableQuantityOn(new DateOnly(2026, 11, 11)));
    }
    [Fact] public void ExpiredReceiptIsRejected() => Assert.Throws<WarehouseDomainException>(() => Item(0).ReceiveLot(Guid.NewGuid(), 1, new LotCode("OLD"), new StorageLocation("A"), new DateOnly(2026, 11, 9), new StockQuantity(1), Actor, Now));
    [Fact]
    public void IdempotentReplayDoesNotDeductTwice()
    {
        var item = Item(); var lot = item.Lots.Single(); var operation = Guid.NewGuid(); var expected = item.Version;
        item.RecordMovement(operation, expected, lot.Id, MovementType.Issue, 4, new MovementReason("SALE-1"), Actor, Now);
        item.RecordMovement(operation, expected, lot.Id, MovementType.Issue, 4, new MovementReason("SALE-1"), Actor, Now.AddMinutes(1));
        Assert.Equal(16, item.TotalQuantity); Assert.Equal(expected + 1, item.Version); Assert.Equal(2, item.Movements.Count);
        Assert.Throws<WarehouseDomainException>(() => item.RecordMovement(operation, item.Version, lot.Id, MovementType.Issue, 5, new MovementReason("SALE-1"), Actor, Now));
    }
    [Fact]
    public void StaleVersionCannotWrite()
    {
        var item = Item(); Assert.Throws<WarehouseDomainException>(() => item.RecordMovement(Guid.NewGuid(), 1, item.Lots.Single().Id, MovementType.Waste, 1, new MovementReason("Damage"), Actor, Now));
        Assert.Equal(20, item.TotalQuantity);
    }
    [Fact]
    public void CountCapturesDifferenceAndReason()
    {
        var item = Item(); item.RegisterCount(Guid.NewGuid(), item.Version, item.Lots.Single().Id, new StockQuantity(20), new StockQuantity(18), new MovementReason("Physical count"), Actor, Now);
        Assert.Equal(18, item.TotalQuantity); Assert.Single(item.Counts); Assert.Equal(-2, item.Movements.Last().Change);
    }
    [Fact]
    public void CountWithChangedBaselineIsRejected()
    {
        var item = Item(); Assert.Throws<WarehouseDomainException>(() => item.RegisterCount(Guid.NewGuid(), item.Version, item.Lots.Single().Id, new StockQuantity(19), new StockQuantity(18), new MovementReason("Count"), Actor, Now)); Assert.Empty(item.Counts);
    }
    [Fact]
    public void CountWithoutDifferenceDoesNotInventAMovement()
    {
        var item = Item(); item.RegisterCount(Guid.NewGuid(), item.Version, item.Lots.Single().Id, new StockQuantity(20), new StockQuantity(20), new MovementReason("Count"), Actor, Now); Assert.Single(item.Movements); Assert.Single(item.Counts);
    }
    [Fact]
    public void QuarantinedReturnsGetASeparateLot()
    {
        var item = Item(); var original = item.Lots.Single(); item.ClassifyReturn(Guid.NewGuid(), item.Version, original.Id, new StockQuantity(2), ReturnDisposition.Quarantine, "SALE-1", new MovementReason("Inspect return"), Actor, Now);
        Assert.Equal(22, item.TotalQuantity); Assert.Equal(20, item.AvailableQuantityOn(new DateOnly(2026, 11, 10))); Assert.Equal(20, original.Quantity.Value); Assert.False(original.IsQuarantined); Assert.Equal(2, item.Lots.Count);
    }
    [Fact]
    public void RejectedReturnNeverAddsToStock()
    {
        var item = Item(); item.ClassifyReturn(Guid.NewGuid(), item.Version, item.Lots.Single().Id, new StockQuantity(2), ReturnDisposition.Rejected, "SALE-1", new MovementReason("Damaged"), Actor, Now); Assert.Equal(20, item.TotalQuantity); Assert.Single(item.Returns); Assert.Single(item.Movements);
    }
    [Fact]
    public void RestockReturnAddsAvailableStock()
    {
        var item = Item(); item.ClassifyReturn(Guid.NewGuid(), item.Version, item.Lots.Single().Id, new StockQuantity(2), ReturnDisposition.Restock, "SALE-1", new MovementReason("Inspected"), Actor, Now); Assert.Equal(22, item.TotalQuantity); Assert.Equal(22, item.AvailableQuantityOn(new DateOnly(2026, 11, 10)));
    }
    [Fact]
    public void WasteCanRemoveQuarantinedStock()
    {
        var item = Item(); var lot = item.Lots.Single(); item.SetQuarantine(Guid.NewGuid(), item.Version, lot.Id, true, new MovementReason("Inspection"), Actor, Now); item.RecordMovement(Guid.NewGuid(), item.Version, lot.Id, MovementType.Waste, 2, new MovementReason("Spoiled"), Actor, Now); Assert.Equal(18, item.TotalQuantity);
    }
    [Fact]
    public void SameLotAndLocationCannotChangeExpiry()
    {
        var item = Item(); var lot = item.Lots.Single(); Assert.Throws<WarehouseDomainException>(() => item.ReceiveLot(Guid.NewGuid(), item.Version, lot.Code, lot.Location, new DateOnly(2027, 1, 1), new StockQuantity(1), Actor, Now)); Assert.Equal(20, item.TotalQuantity);
    }
    [Fact]
    public void RelocationPreservesQuantityAndRejectsMergingLocations()
    {
        var item = Item(); var first = item.Lots.Single(); item.ReceiveLot(Guid.NewGuid(), item.Version, first.Code, new StorageLocation("B"), first.ExpiresOn, new StockQuantity(2), Actor, Now);
        Assert.Throws<WarehouseDomainException>(() => item.RelocateLot(Guid.NewGuid(), item.Version, first.Id, new StorageLocation("B"), new MovementReason("Move"), Actor, Now));
        item.RelocateLot(Guid.NewGuid(), item.Version, first.Id, new StorageLocation("C"), new MovementReason("Move"), Actor, Now); Assert.Equal(22, item.TotalQuantity); Assert.Equal("C", first.Location.Value);
    }
    [Fact]
    public void AggregateCollectionsCannotBeModifiedByCallers()
    {
        var item = Item(); Assert.Throws<NotSupportedException>(() => ((ICollection<AiToDu.Platform.Warehouse.Domain.Model.Entities.StockLot>)item.Lots).Clear());
        Assert.All(typeof(AiToDu.Platform.Warehouse.Domain.Model.Entities.StockLot).GetProperties(), p => Assert.False(p.SetMethod?.IsPublic ?? false));
    }
    [Fact]
    public void DomainHasNoDependencyOnOuterLayers()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, "AiToDu.Platform", "Warehouse"))) directory = directory.Parent;
        Assert.NotNull(directory);
        foreach (var file in Directory.EnumerateFiles(Path.Combine(directory!.FullName, "AiToDu.Platform", "Warehouse", "Domain"), "*.cs", SearchOption.AllDirectories))
        {
            var source = File.ReadAllText(file); Assert.DoesNotContain("using Microsoft", source); Assert.DoesNotContain(".Infrastructure", source); Assert.DoesNotContain(".Interfaces", source); Assert.DoesNotContain(".Application", source);
        }
    }
}
