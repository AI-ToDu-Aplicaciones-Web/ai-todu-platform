using System.Net;
using AiToDu.Platform.IntegrationTests.Infrastructure;

namespace AiToDu.Platform.IntegrationTests.Sales;

[Collection(ApiCollection.Name)]
public sealed class SalesCheckoutTests(AiToDuApiFactory factory)
{
    [Fact(DisplayName = "Ventas · Reenviar el mismo operationId conserva una sola venta y descuenta el stock una vez")]
    public async Task CheckoutRetry_IsIdempotent()
    {
        var api = await ApiSession.SignUpOwnerAsync(factory, "Ventas Reintento Idempotente");
        var item = await api.CreateItem(ApiSession.NewSku("IDEM"));
        item = await api.Receive(item, "LOTE-1", "A-01", 10, null);
        var customer = await api.RegisterCustomer("Cliente Reintento");
        var operationId = Guid.NewGuid();
        var requestItems = new[] { new CheckoutLineRequest(item.Id, 3) };

        var first = await ApiSession.Read<SaleDto>(await api.TryCheckout(operationId, customer.Id,
            requestItems, paymentAccepted: true, paymentReference: "PAGO-IDEMPOTENTE"), HttpStatusCode.OK);
        var retry = await ApiSession.Read<SaleDto>(await api.TryCheckout(operationId, customer.Id,
            requestItems, paymentAccepted: true, paymentReference: "PAGO-IDEMPOTENTE"), HttpStatusCode.OK);

        Assert.Equal(operationId, first.OperationId);
        Assert.Equal(first.Id, retry.Id);
        Assert.Equal("Paid", retry.Status);
        Assert.Single(retry.Lines);
        Assert.Equal(7m, (await api.GetItem(item.Id)).AvailableQuantity);
        Assert.Single((await api.GetSalesHistory()).Where(x => x.OperationId == operationId));
    }

    [Fact(DisplayName = "Ventas · Reutilizar operationId con otro carrito se rechaza sin otra venta ni descuento")]
    public async Task CheckoutOperationId_WithChangedCart_IsRejectedWithoutSideEffects()
    {
        var api = await ApiSession.SignUpOwnerAsync(factory, "Ventas Referencia Reutilizada");
        var item = await api.CreateItem(ApiSession.NewSku("REFE"));
        item = await api.Receive(item, "LOTE-1", "A-01", 10, null);
        var customer = await api.RegisterCustomer("Cliente Referencia Reutilizada");
        var operationId = Guid.NewGuid();

        await ApiSession.Read<SaleDto>(await api.TryCheckout(operationId, customer.Id,
            [new(item.Id, 3)], paymentAccepted: true, paymentReference: "PAGO-REFERENCIA"), HttpStatusCode.OK);
        await ApiSession.ExpectStatus(await api.TryCheckout(operationId, customer.Id,
            [new(item.Id, 4)], paymentAccepted: true, paymentReference: "PAGO-REFERENCIA"), HttpStatusCode.Conflict);

        Assert.Equal(7m, (await api.GetItem(item.Id)).AvailableQuantity);
        Assert.Single((await api.GetSalesHistory()).Where(x => x.OperationId == operationId));
    }

    [Fact(DisplayName = "Ventas · El carrito falla en la segunda línea y revierte la primera salida de inventario")]
    public async Task CheckoutWithInsufficientSecondLine_RollsBackTheWholeCart()
    {
        var api = await ApiSession.SignUpOwnerAsync(factory, "Ventas Reversión de Carrito");
        var firstItem = await api.CreateItem(ApiSession.NewSku("PRIM"));
        firstItem = await api.Receive(firstItem, "LOTE-1", "A-01", 10, null);
        var secondItem = await api.CreateItem(ApiSession.NewSku("SEGU"));
        secondItem = await api.Receive(secondItem, "LOTE-2", "A-02", 2, null);
        var customer = await api.RegisterCustomer("Cliente Carrito Incompleto");
        var operationId = Guid.NewGuid();

        var response = await api.TryCheckout(operationId, customer.Id,
            [new(firstItem.Id, 3), new(secondItem.Id, 3)], paymentAccepted: true, paymentReference: "PAGO-CARRITO");

        await ApiSession.ExpectStatus(response, HttpStatusCode.Conflict);
        Assert.Equal(10m, (await api.GetItem(firstItem.Id)).AvailableQuantity);
        Assert.Equal(2m, (await api.GetItem(secondItem.Id)).AvailableQuantity);
        Assert.DoesNotContain((await api.GetSalesHistory()), x => x.OperationId == operationId);
    }

    [Fact(DisplayName = "Ventas · Un pago rechazado queda pendiente; cobrar después genera un comprobante y descuenta una vez")]
    public async Task RejectedCheckout_CanBePaidOnceAndReceiptedFromHistory()
    {
        var api = await ApiSession.SignUpOwnerAsync(factory, "Ventas Ciclo de Pago");
        var item = await api.CreateItem(ApiSession.NewSku("PAGO"));
        item = await api.Receive(item, "LOTE-1", "A-01", 10, null);
        var customer = await api.RegisterCustomer("Cliente Ciclo de Pago");
        var operationId = Guid.NewGuid();
        var requestItems = new[] { new CheckoutLineRequest(item.Id, 3) };

        var pending = await ApiSession.Read<SaleDto>(await api.TryCheckout(operationId, customer.Id,
            requestItems, paymentAccepted: false, paymentReference: "PAGO-RECHAZADO"), HttpStatusCode.OK);
        Assert.Equal("Pending", pending.Status);
        Assert.Null(pending.Receipt);
        Assert.Equal(10m, (await api.GetItem(item.Id)).AvailableQuantity);
        await ApiSession.ExpectStatus(await api.Http.GetAsync($"/api/v1/transactions/{pending.Id}/receipt"), HttpStatusCode.Conflict);

        const string acceptedReference = "PAGO-ACEPTADO";
        var paid = await ApiSession.Read<SaleDto>(await api.TryPay(pending.Id, acceptedReference), HttpStatusCode.OK);
        Assert.Equal("Paid", paid.Status);
        Assert.NotNull(paid.Receipt);
        Assert.Equal(7m, (await api.GetItem(item.Id)).AvailableQuantity);

        // Retrying an acknowledged payment must not issue the stock for a second time.
        var paymentRetry = await ApiSession.Read<SaleDto>(await api.TryPay(pending.Id, acceptedReference), HttpStatusCode.OK);
        Assert.Equal(paid.Receipt, paymentRetry.Receipt);
        Assert.Equal(7m, (await api.GetItem(item.Id)).AvailableQuantity);

        var history = await api.GetSalesHistory();
        var saleFromHistory = Assert.Single(history, x => x.OperationId == operationId);
        var receipt = await api.GetReceipt(saleFromHistory.Id);
        Assert.Equal(paid.Receipt, receipt.Receipt);
        Assert.Equal(paid.Total, receipt.Total);
    }
}
