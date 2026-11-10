using AiToDu.Platform.Shared.Domain.Model.ValueObjects;
using AiToDu.Platform.Shared.Domain.Repositories;
using AiToDu.Platform.Warehouse.Application.Internal.CommandServices;
using AiToDu.Platform.Warehouse.Application.OutboundServices;
using AiToDu.Platform.Warehouse.Domain.Model.Aggregates;
using AiToDu.Platform.Warehouse.Domain.Model.Commands;
using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
using AiToDu.Platform.Warehouse.Domain.Repositories;
using Xunit;

namespace AiToDu.Platform.Tests;

public sealed class WarehouseApplicationPolicyTests
{
    private sealed class Actor(bool manager, bool seller) : ICurrentWarehouseActor
    {
        public OrganizationId OrganizationId { get; } = new(Guid.NewGuid());
        public ActorId ActorId { get; } = new(Guid.NewGuid());
        public bool CanManageInventory => manager;
        public bool CanRecordSales => seller;
    }
    private sealed class Clock : IWarehouseClock { public DateTimeOffset GetCurrentTime() => new(2026, 11, 10, 12, 0, 0, TimeSpan.FromHours(-5)); }
    private sealed class Work : IUnitOfWork { public int Commits { get; private set; } public Task CompleteAsync(CancellationToken cancellationToken = default) { Commits++; return Task.CompletedTask; } }
    private sealed class Repository(InventoryItem item) : IInventoryItemRepository
    {
        public Task AddAsync(InventoryItem entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<InventoryItem?> FindAsync(OrganizationId organizationId, InventoryItemId itemId, bool tracking, CancellationToken ct) => Task.FromResult<InventoryItem?>(item.OrganizationId == organizationId && item.Id == itemId ? item : null);
        public Task<bool> SkuExistsAsync(OrganizationId organizationId, Sku sku, CancellationToken ct) => Task.FromResult(item.OrganizationId == organizationId && item.Sku == sku);
        public Task<(IReadOnlyList<InventoryItem> Items, int Total)> SearchAsync(OrganizationId organizationId, string? search, int page, int pageSize, CancellationToken ct) => throw new NotSupportedException();
    }
    [Theory]
    [InlineData(false, true, true)]
    [InlineData(false, false, false)]
    [InlineData(true, true, true)]
    public async Task SalesUseCaseHasAnExplicitPolicyWithoutGrantingGeneralInventoryWrites(bool manager, bool seller, bool expected)
    {
        var actor = new Actor(manager, seller); var clock = new Clock(); var work = new Work();
        var item = new InventoryItem(InventoryItemId.New(), actor.OrganizationId, new Sku("SKU-1"), "Rice", "Food", new StockQuantity(5), 10, actor.ActorId, clock.GetCurrentTime());
        item.ReceiveLot(Guid.NewGuid(), item.Version, new LotCode("A"), new StorageLocation("A"), null, new StockQuantity(20), actor.ActorId, clock.GetCurrentTime());
        var service = new InventoryItemCommandService(new Repository(item), work, actor, clock);
        if (!manager)
        {
            var adjustment = await service.Handle(new RecordInventoryMovementCommand(item.Id, Guid.NewGuid(), item.Version, item.Lots.Single().Id, MovementType.Adjustment, 10, "Manual adjustment"), CancellationToken.None);
            Assert.False(adjustment.IsSuccess); Assert.Equal(20, item.TotalQuantity);
        }
        var sale = await service.Handle(new IssueStockForSaleCommand(item.Id, Guid.NewGuid(), item.Version, item.Lots.Single().Id, 4, "SALE-1"), CancellationToken.None);
        Assert.Equal(expected, sale.IsSuccess); Assert.Equal(expected ? 16 : 20, item.TotalQuantity); Assert.Equal(expected ? 1 : 0, work.Commits);
    }
}
