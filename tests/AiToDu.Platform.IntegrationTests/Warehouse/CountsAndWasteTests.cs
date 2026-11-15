using System.Net;
using AiToDu.Platform.IntegrationTests.Infrastructure;
using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
using AiToDu.Platform.Warehouse.Interfaces.Rest.Resources;

namespace AiToDu.Platform.IntegrationTests.Warehouse;

/// <summary>S2-04: physical counts, adjustments and justified waste, with movement consistency.</summary>
[Collection(ApiCollection.Name)]
public sealed class CountsAndWasteTests(AiToDuApiFactory factory)
{
    [Fact(DisplayName = "S2-04 · Conteo de 8 y merma de 1 sobre 10 dejan saldo 7, con sus movimientos, y se rechaza una versión antigua")]
    public async Task CountAndWaste_LeaveSevenAndRejectStaleVersion()
    {
        var api = await ApiSession.SignUpOwnerAsync(factory, "Bodega Conteos");
        var item = await api.CreateItem(ApiSession.NewSku("CONT"));
        item = await api.Receive(item, "L-010", "A-01", 10, null);
        var lot = Assert.Single(item.Lots);
        var versionAfterReceipt = item.Version;

        var countOperation = Guid.NewGuid();
        item = await ApiSession.Read<InventoryItemResource>(await api.TryCount(item.Id,
            new RegisterStockCountResource(countOperation, item.Version, lot.Id, 10, 8, "Conteo físico mensual")), HttpStatusCode.OK);
        Assert.Equal(8m, item.TotalQuantity);

        var wasteOperation = Guid.NewGuid();
        item = await ApiSession.Read<InventoryItemResource>(await api.TryMovement(item.Id,
            new RecordInventoryMovementResource(wasteOperation, item.Version, lot.Id, MovementType.Waste, 1, "Botella rota en anaquel")), HttpStatusCode.OK);
        Assert.Equal(7m, item.TotalQuantity);

        // An update built from an old version must be rejected and must not touch the stock.
        var stale = await api.TryMovement(item.Id, new RecordInventoryMovementResource(
            Guid.NewGuid(), versionAfterReceipt, lot.Id, MovementType.Waste, 1, "Merma con versión antigua"));
        await ApiSession.ExpectStatus(stale, HttpStatusCode.Conflict);

        var reloaded = await api.GetItem(item.Id);
        Assert.Equal(item.Version, reloaded.Version);
        Assert.Equal(7m, reloaded.TotalQuantity);
        Assert.Equal(7m, reloaded.AvailableQuantity);
        Assert.Equal(7m, Assert.Single(reloaded.Lots).Quantity);

        var count = Assert.Single(reloaded.Counts);
        Assert.Equal(10m, count.ExpectedQuantity);
        Assert.Equal(8m, count.CountedQuantity);

        Assert.Equal(3, reloaded.Movements.Count);
        var receipt = Assert.Single(reloaded.Movements, x => x.Type == nameof(MovementType.Receipt));
        Assert.Equal((0m, 10m), (receipt.Before, receipt.After));
        var countAdjustment = Assert.Single(reloaded.Movements, x => x.Type == nameof(MovementType.CountAdjustment));
        Assert.Equal((10m, 8m, -2m), (countAdjustment.Before, countAdjustment.After, countAdjustment.Change));
        Assert.Equal(countOperation, countAdjustment.OperationId);
        var waste = Assert.Single(reloaded.Movements, x => x.Type == nameof(MovementType.Waste));
        Assert.Equal((8m, 7m, -1m), (waste.Before, waste.After, waste.Change));
        Assert.Equal(wasteOperation, waste.OperationId);
        Assert.Equal("Botella rota en anaquel", waste.Reason);
        Assert.All(reloaded.Movements, x => Assert.Equal(lot.Id, x.LotId));
    }

    [Fact(DisplayName = "S2-04 · Reintentar la misma merma no descuenta dos veces; reutilizar su operationId con otros datos se rechaza")]
    public async Task RetriedWaste_IsAppliedOnce()
    {
        var api = await ApiSession.SignUpOwnerAsync(factory, "Bodega Reintentos");
        var item = await api.CreateItem(ApiSession.NewSku("REIN"));
        item = await api.Receive(item, "L-020", "A-01", 10, null);
        var lot = Assert.Single(item.Lots);

        var waste = new RecordInventoryMovementResource(Guid.NewGuid(), item.Version, lot.Id, MovementType.Waste, 1, "Producto vencido en tienda");
        await ApiSession.ExpectStatus(await api.TryMovement(item.Id, waste), HttpStatusCode.OK);
        await ApiSession.ExpectStatus(await api.TryMovement(item.Id, waste), HttpStatusCode.OK);
        await ApiSession.ExpectStatus(await api.TryMovement(item.Id, waste with { Quantity = 2 }), HttpStatusCode.Conflict);

        var reloaded = await api.GetItem(item.Id);
        Assert.Equal(9m, reloaded.TotalQuantity);
        Assert.Single(reloaded.Movements, x => x.Type == nameof(MovementType.Waste));
    }

    [Fact(DisplayName = "S2-04 · Se rechaza una merma sin motivo y un conteo cuyo saldo esperado ya no coincide")]
    public async Task WasteWithoutReasonAndOutdatedCount_AreRejected()
    {
        var api = await ApiSession.SignUpOwnerAsync(factory, "Bodega Validaciones");
        var item = await api.CreateItem(ApiSession.NewSku("VALI"));
        item = await api.Receive(item, "L-030", "A-01", 10, null);
        var lot = Assert.Single(item.Lots);

        var noReason = await api.TryMovement(item.Id, new RecordInventoryMovementResource(
            Guid.NewGuid(), item.Version, lot.Id, MovementType.Waste, 1, "   "));
        await ApiSession.ExpectStatus(noReason, HttpStatusCode.BadRequest);

        var outdatedCount = await api.TryCount(item.Id, new RegisterStockCountResource(
            Guid.NewGuid(), item.Version, lot.Id, 9, 8, "Conteo con saldo desactualizado"));
        await ApiSession.ExpectStatus(outdatedCount, HttpStatusCode.Conflict);

        var reloaded = await api.GetItem(item.Id);
        Assert.Equal(10m, reloaded.TotalQuantity);
        Assert.Empty(reloaded.Counts);
        Assert.Single(reloaded.Movements);
    }

    [Fact(DisplayName = "S2-04 · Un ajuste puede sumar o restar stock y queda registrado con su motivo")]
    public async Task Adjustments_CanIncreaseAndDecreaseStock()
    {
        var api = await ApiSession.SignUpOwnerAsync(factory, "Bodega Ajustes");
        var item = await api.CreateItem(ApiSession.NewSku("AJUS"));
        item = await api.Receive(item, "L-050", "A-01", 10, null);
        var lot = Assert.Single(item.Lots);

        item = await ApiSession.Read<InventoryItemResource>(await api.TryMovement(item.Id, new RecordInventoryMovementResource(
            Guid.NewGuid(), item.Version, lot.Id, MovementType.Adjustment, 2, "Unidades encontradas en trastienda")), HttpStatusCode.OK);
        item = await ApiSession.Read<InventoryItemResource>(await api.TryMovement(item.Id, new RecordInventoryMovementResource(
            Guid.NewGuid(), item.Version, lot.Id, MovementType.Adjustment, -1, "Error de digitación en la recepción")), HttpStatusCode.OK);

        var reloaded = await api.GetItem(item.Id);
        Assert.Equal(11m, reloaded.TotalQuantity);
        var adjustments = reloaded.Movements.Where(x => x.Type == nameof(MovementType.Adjustment)).ToList();
        Assert.Equal(2, adjustments.Count);
        Assert.Contains(adjustments, x => x.Change == 2m && x.Reason == "Unidades encontradas en trastienda");
        Assert.Contains(adjustments, x => x.Change == -1m && x.Reason == "Error de digitación en la recepción");
    }

    [Fact(DisplayName = "S2-04 · Una merma mayor al stock del lote se rechaza sin tocar el saldo")]
    public async Task WasteBeyondStock_IsRejected()
    {
        var api = await ApiSession.SignUpOwnerAsync(factory, "Bodega Sobremerma");
        var item = await api.CreateItem(ApiSession.NewSku("SOBR"));
        item = await api.Receive(item, "L-060", "A-01", 3, null);
        var lot = Assert.Single(item.Lots);

        var tooMuch = await api.TryMovement(item.Id, new RecordInventoryMovementResource(
            Guid.NewGuid(), item.Version, lot.Id, MovementType.Waste, 5, "Merma mayor al stock"));
        await ApiSession.ExpectStatus(tooMuch, HttpStatusCode.Conflict);

        var reloaded = await api.GetItem(item.Id);
        Assert.Equal(3m, reloaded.TotalQuantity);
        Assert.Single(reloaded.Movements);
    }

    [Fact(DisplayName = "S2-04 · Un ajuste de cero unidades se rechaza")]
    public async Task ZeroAdjustment_IsRejected()
    {
        var api = await ApiSession.SignUpOwnerAsync(factory, "Bodega Ajuste Cero");
        var item = await api.CreateItem(ApiSession.NewSku("CERO"));
        item = await api.Receive(item, "L-070", "A-01", 10, null);
        var lot = Assert.Single(item.Lots);

        var zero = await api.TryMovement(item.Id, new RecordInventoryMovementResource(
            Guid.NewGuid(), item.Version, lot.Id, MovementType.Adjustment, 0, "Ajuste sin cambio"));
        await ApiSession.ExpectStatus(zero, HttpStatusCode.BadRequest);

        var reloaded = await api.GetItem(item.Id);
        Assert.Equal(10m, reloaded.TotalQuantity);
        Assert.Single(reloaded.Movements);
    }
}   