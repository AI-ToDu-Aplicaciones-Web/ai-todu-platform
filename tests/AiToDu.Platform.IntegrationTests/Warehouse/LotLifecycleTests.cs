using System.Net;
using System.Net.Http.Json;
using AiToDu.Platform.IntegrationTests.Infrastructure;
using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
using AiToDu.Platform.Warehouse.Interfaces.Rest.Resources;

namespace AiToDu.Platform.IntegrationTests.Warehouse;

/// <summary>S2-03: products, SKU lookup, location, lots, expiry and quarantine.</summary>
[Collection(ApiCollection.Name)]
public sealed class LotLifecycleTests(AiToDuApiFactory factory)
{
    [Fact(DisplayName = "S2-03 · Lote: recibir 10, buscar por SKU, reubicar y poner en cuarentena deja 10 físicas y 0 disponibles")]
    public async Task LotLifecycle_KeepsPhysicalStockButRemovesAvailability()
    {
        var api = await ApiSession.SignUpOwnerAsync(factory, "Bodega Ciclo de Lote");
        var sku = ApiSession.NewSku("LOTE");

        var item = await api.CreateItem(sku);
        Assert.Equal(0m, item.TotalQuantity);

        item = await api.Receive(item, "L-001", "A-01", 10, factory.Today.AddDays(30));
        var lot = Assert.Single(item.Lots);
        Assert.Equal(10m, item.TotalQuantity);
        Assert.Equal(10m, item.AvailableQuantity);

        // Locate it by SKU (lookup is case-insensitive) and through the search used by the SPA.
        var bySku = await ApiSession.Read<InventoryItemResource>(
            await api.Http.GetAsync($"{ApiSession.InventoryUrl}/by-sku/{sku.ToLowerInvariant()}"), HttpStatusCode.OK);
        Assert.Equal(item.Id, bySku.Id);
        var page = await ApiSession.Read<InventoryPageResource>(
            await api.Http.GetAsync($"{ApiSession.InventoryUrl}?search={sku}"), HttpStatusCode.OK);
        Assert.Contains(page.Items, x => x.Id == item.Id);

        item = await ApiSession.Read<InventoryItemResource>(
            await api.TryRelocate(item, lot.Id, "B-02", "Reordenamiento del almacén"), HttpStatusCode.OK);

        item = await ApiSession.Read<InventoryItemResource>(
            await api.TrySetQuarantine(item, lot.Id, true, "Empaque dañado en recepción"), HttpStatusCode.OK);

        // A quarantined lot cannot be issued.
        var issue = await api.TryMovement(item.Id, new RecordInventoryMovementResource(
            Guid.NewGuid(), item.Version, lot.Id, MovementType.Issue, 1, "Salida de prueba"));
        await ApiSession.ExpectStatus(issue, HttpStatusCode.Conflict);

        // Query the API again: the state must come from the database, not from the last response.
        var reloaded = await api.GetItem(item.Id);
        Assert.Equal(10m, reloaded.TotalQuantity);
        Assert.Equal(0m, reloaded.AvailableQuantity);
        Assert.Equal("STOCK_OUT", reloaded.Status);
        var reloadedLot = Assert.Single(reloaded.Lots);
        Assert.Equal(lot.Id, reloadedLot.Id);
        Assert.Equal("B-02", reloadedLot.Location);
        Assert.True(reloadedLot.IsQuarantined);
        Assert.Equal(10m, reloadedLot.Quantity);
        var receipt = Assert.Single(reloaded.Movements);
        Assert.Equal(nameof(MovementType.Receipt), receipt.Type);
        Assert.Equal(0m, receipt.Before);
        Assert.Equal(10m, receipt.After);
    }

    [Fact(DisplayName = "S2-03 · SKU duplicado: se rechaza en la misma organización (sin importar mayúsculas) y se permite en otra")]
    public async Task DuplicateSku_IsRejectedWithinOrganizationOnly()
    {
        var api = await ApiSession.SignUpOwnerAsync(factory, "Bodega SKU Duplicado");
        var sku = ApiSession.NewSku("DUP");
        var original = await api.CreateItem(sku);

        await ApiSession.ExpectStatus(await api.TryCreateItem(sku), HttpStatusCode.Conflict);
        await ApiSession.ExpectStatus(await api.TryCreateItem($"  {sku.ToLowerInvariant()} "), HttpStatusCode.Conflict);

        var page = await ApiSession.Read<InventoryPageResource>(
            await api.Http.GetAsync($"{ApiSession.InventoryUrl}?search={sku}"), HttpStatusCode.OK);
        Assert.Equal(original.Id, Assert.Single(page.Items).Id);

        var otherOrganization = await ApiSession.SignUpOwnerAsync(factory, "Otra Bodega");
        await otherOrganization.CreateItem(sku);
    }

    [Fact(DisplayName = "S2-03 · Lote vencido: no se recibe si ya venció y, al vencer, deja de estar disponible")]
    public async Task ExpiredLot_IsNotReceivedAndStopsBeingAvailable()
    {
        var api = await ApiSession.SignUpOwnerAsync(factory, "Bodega Vencimientos");
        var item = await api.CreateItem(ApiSession.NewSku("VENC"));

        var expiredReceipt = await api.TryReceive(item, "L-VENCIDO", "A-01", 5, factory.Today.AddDays(-1));
        await ApiSession.ExpectStatus(expiredReceipt, HttpStatusCode.BadRequest);
        var unchanged = await api.GetItem(item.Id);
        Assert.Empty(unchanged.Lots);
        Assert.Equal(item.Version, unchanged.Version);

        item = await api.Receive(item, "L-CORTO", "A-01", 5, factory.Today.AddDays(1));
        var lot = Assert.Single(item.Lots);
        Assert.Equal(5m, item.AvailableQuantity);

        factory.Clock.Advance(TimeSpan.FromDays(2));
        try
        {
            var expired = await api.GetItem(item.Id);
            var expiredLot = Assert.Single(expired.Lots);
            Assert.True(expiredLot.IsExpired);
            Assert.Equal(5m, expired.TotalQuantity);
            Assert.Equal(0m, expired.AvailableQuantity);

            var issue = await api.TryMovement(item.Id, new RecordInventoryMovementResource(
                Guid.NewGuid(), expired.Version, lot.Id, MovementType.Issue, 1, "Salida de lote vencido"));
            await ApiSession.ExpectStatus(issue, HttpStatusCode.Conflict);
            Assert.Equal(5m, (await api.GetItem(item.Id)).TotalQuantity);
        }
        finally
        {
            factory.Clock.Reset();
        }
    }

    [Fact(DisplayName = "S2-03 · Recibir otra vez el mismo lote en la misma ubicación suma unidades sin crear otro lote")]
    public async Task SameLotAndLocation_AccumulatesQuantity()
    {
        var api = await ApiSession.SignUpOwnerAsync(factory, "Bodega Reposición");
        var item = await api.CreateItem(ApiSession.NewSku("REPO"));
        var expiry = factory.Today.AddDays(30);

        item = await api.Receive(item, "L-100", "A-01", 10, expiry);
        item = await api.Receive(item, "l-100", "A-01", 5, expiry);

        var reloaded = await api.GetItem(item.Id);
        var lot = Assert.Single(reloaded.Lots);
        Assert.Equal("L-100", lot.Code);
        Assert.Equal(15m, lot.Quantity);
        Assert.Equal(2, reloaded.Movements.Count(x => x.Type == nameof(MovementType.Receipt)));
    }

    [Fact(DisplayName = "S2-03 · El mismo lote en la misma ubicación no puede tener dos fechas de vencimiento")]
    public async Task SameLotWithDifferentExpiry_IsRejected()
    {
        var api = await ApiSession.SignUpOwnerAsync(factory, "Bodega Fechas");
        var item = await api.CreateItem(ApiSession.NewSku("FECH"));
        item = await api.Receive(item, "L-200", "A-01", 10, factory.Today.AddDays(30));

        var conflict = await api.TryReceive(item, "L-200", "A-01", 5, factory.Today.AddDays(45));
        await ApiSession.ExpectStatus(conflict, HttpStatusCode.Conflict);

        var reloaded = await api.GetItem(item.Id);
        Assert.Equal(10m, Assert.Single(reloaded.Lots).Quantity);
    }

    [Fact(DisplayName = "S2-03 · No se puede mover un lote a una ubicación donde ya está ese mismo lote")]
    public async Task RelocatingOntoSameLotCode_IsRejected()
    {
        var api = await ApiSession.SignUpOwnerAsync(factory, "Bodega Ubicaciones");
        var item = await api.CreateItem(ApiSession.NewSku("UBIC"));
        item = await api.Receive(item, "L-300", "A-01", 4, null);
        item = await api.Receive(item, "L-300", "B-01", 6, null);
        Assert.Equal(2, item.Lots.Count);
        var second = Assert.Single(item.Lots, x => x.Location == "B-01");

        var conflict = await api.TryRelocate(item, second.Id, "A-01", "Juntar lotes");
        await ApiSession.ExpectStatus(conflict, HttpStatusCode.Conflict);

        var reloaded = await api.GetItem(item.Id);
        Assert.Equal("B-01", reloaded.Lots.Single(x => x.Id == second.Id).Location);
        Assert.Equal(10m, reloaded.TotalQuantity);
    }
}