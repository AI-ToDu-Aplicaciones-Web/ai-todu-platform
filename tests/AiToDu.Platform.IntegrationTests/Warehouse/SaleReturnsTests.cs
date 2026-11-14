using System.Net;
using AiToDu.Platform.IntegrationTests.Infrastructure;
using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
using AiToDu.Platform.Warehouse.Interfaces.Rest.Resources;

namespace AiToDu.Platform.IntegrationTests.Warehouse;

/// <summary>S2-04: justified customer returns (restock, quarantine, rejected) on real sale lines.</summary>
[Collection(ApiCollection.Name)]
public sealed class SaleReturnsTests(AiToDuApiFactory factory)
{
    [Fact(DisplayName = "S2-04 · Devoluciones: reingreso, cuarentena y rechazo sobre una venta real, sin duplicarse al repetirlas")]
    public async Task ThreeReturnDispositions_OnARealSale()
    {
        var api = await ApiSession.SignUpOwnerAsync(factory, "Bodega Devoluciones");
        var item = await api.CreateItem(ApiSession.NewSku("DEVO"));
        item = await api.Receive(item, "L-040", "A-03", 10, factory.Today.AddDays(60));
        var lot = Assert.Single(item.Lots);

        var customer = await api.RegisterCustomer("Cliente Devoluciones");
        var sale = await api.PaidSale(customer.Id, item.Id, 3);
        Assert.Equal("Paid", sale.Status);
        Assert.NotNull(sale.Receipt);
        var line = Assert.Single(sale.Lines);
        Assert.Equal((item.Id, lot.Id, 3m), (line.ItemId, line.LotId, line.Quantity));
        Assert.Equal(7m, (await api.GetItem(item.Id)).AvailableQuantity);

        // 1) Restock: the unit goes back to the same lot and is available again.
        var restock = ReturnOf(sale, lot.Id, ReturnDisposition.Restock, "Cliente se arrepintió, empaque intacto");
        await ApiSession.ExpectStatus(await api.TryReturn(item.Id, restock), HttpStatusCode.OK);
        item = await api.GetItem(item.Id);
        Assert.Equal(8m, item.Lots.Single(x => x.Id == lot.Id).Quantity);
        Assert.Equal(8m, item.AvailableQuantity);

        // 2) Quarantine: the unit goes to a new quarantined lot, so it is physical but not available.
        var quarantine = ReturnOf(sale, lot.Id, ReturnDisposition.Quarantine, "Caja golpeada, revisar calidad");
        await ApiSession.ExpectStatus(await api.TryReturn(item.Id, quarantine), HttpStatusCode.OK);
        item = await api.GetItem(item.Id);
        var quarantineLot = Assert.Single(item.Lots, x => x.Id != lot.Id);
        Assert.True(quarantineLot.IsQuarantined);
        Assert.Equal(1m, quarantineLot.Quantity);
        Assert.Equal("A-03", quarantineLot.Location);
        Assert.StartsWith("RETURN-", quarantineLot.Code);
        Assert.Equal(9m, item.TotalQuantity);
        Assert.Equal(8m, item.AvailableQuantity);

        // 3) Rejected: recorded, but no stock comes back.
        var rejected = ReturnOf(sale, lot.Id, ReturnDisposition.Rejected, "Producto abierto, no apto para venta");
        await ApiSession.ExpectStatus(await api.TryReturn(item.Id, rejected), HttpStatusCode.OK);

        // Repeating the same restock operation must not add stock or a second return.
        await ApiSession.ExpectStatus(await api.TryReturn(item.Id, restock), HttpStatusCode.OK);

        // The sale line is fully returned (3 of 3): a new return is rejected.
        var extra = ReturnOf(sale, lot.Id, ReturnDisposition.Restock, "Devolución en exceso");
        await ApiSession.ExpectStatus(await api.TryReturn(item.Id, extra), HttpStatusCode.Conflict);

        var reloaded = await api.GetItem(item.Id);
        Assert.Equal(9m, reloaded.TotalQuantity);
        Assert.Equal(8m, reloaded.AvailableQuantity);
        Assert.Equal(8m, reloaded.Lots.Single(x => x.Id == lot.Id).Quantity);
        Assert.Equal(1m, reloaded.Lots.Single(x => x.Id == quarantineLot.Id).Quantity);

        Assert.Equal(3, reloaded.Returns.Count);
        Assert.All(reloaded.Returns, x => Assert.Equal((lot.Id, 1m, sale.Id.ToString()), (x.SourceLotId, x.Quantity, x.Reference)));
        Assert.Equal(lot.Id, Assert.Single(reloaded.Returns, x => x.Disposition == nameof(ReturnDisposition.Restock)).DestinationLotId);
        Assert.Equal(quarantineLot.Id, Assert.Single(reloaded.Returns, x => x.Disposition == nameof(ReturnDisposition.Quarantine)).DestinationLotId);
        Assert.Null(Assert.Single(reloaded.Returns, x => x.Disposition == nameof(ReturnDisposition.Rejected)).DestinationLotId);
        Assert.Equal(2, reloaded.Movements.Count(x => x.Type == nameof(MovementType.CustomerReturn)));

        var saleAfter = await api.GetSale(sale.Id);
        Assert.Equal(3, saleAfter.Returns.Count);
        Assert.Equal(3m, Assert.Single(saleAfter.Lines).Returned);
    }

    private static ClassifyInventoryReturnResource ReturnOf(SaleDto sale, Guid lotId, ReturnDisposition disposition, string reason) =>
        new(Guid.NewGuid(), 0, lotId, 1, disposition, sale.Receipt!, reason);
}