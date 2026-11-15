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
}
