using System.Net;
using AiToDu.Platform.IntegrationTests.Infrastructure;
using AiToDu.Platform.Warehouse.Interfaces.Rest.Resources;

namespace AiToDu.Platform.IntegrationTests.Subscriptions;

[Collection(ApiCollection.Name)]
public sealed class SubscriptionLimitsTests(AiToDuApiFactory factory)
{
    [Fact(DisplayName = "Suscripciones · Starter admite tres usuarios y rechaza el cuarto sin guardar el alta")]
    public async Task Starter_RejectsUserBeyondLimitWithoutCreatingIt()
    {
        var api = await ApiSession.SignUpOwnerAsync(factory, "Starter Límite de Usuarios");
        await api.RegisterUser("Operador Uno", NewEmail());
        await api.RegisterUser("Operador Dos", NewEmail());

        var rejectedEmail = NewEmail();
        await ApiSession.ExpectStatus(await api.TryRegisterUser("Operador Extra", rejectedEmail), HttpStatusCode.Conflict);

        var users = await api.GetUsers();
        Assert.Equal(3, users.Count);
        Assert.DoesNotContain(users, user => user.Email == rejectedEmail);
        Assert.Equal("starter", (await api.GetSubscription()).Plan);
    }

    [Fact(DisplayName = "Suscripciones · Starter admite 500 SKU y rechaza el 501 sin guardar el producto")]
    public async Task Starter_RejectsSkuBeyondLimitWithoutCreatingIt()
    {
        var api = await ApiSession.SignUpOwnerAsync(factory, "Starter Límite de SKU");
        for (var index = 0; index < 500; index++)
            await api.CreateItem(ApiSession.NewSku($"S{index:D3}"));

        var before = await ReadInventoryPage(api);
        Assert.Equal(500, before.Total);

        var extraSku = ApiSession.NewSku("EXTR");
        await ApiSession.ExpectStatus(await api.TryCreateItem(extraSku), HttpStatusCode.Conflict);

        var after = await ReadInventoryPage(api);
        Assert.Equal(500, after.Total);
        Assert.DoesNotContain(after.Items, item => item.Sku == extraSku);
        Assert.Equal("starter", (await api.GetSubscription()).Plan);
    }

    private static async Task<InventoryPageResource> ReadInventoryPage(ApiSession api) =>
        await ApiSession.Read<InventoryPageResource>(await api.Http.GetAsync("/api/v1/inventory-items?page=1&pageSize=1"), HttpStatusCode.OK);

    private static string NewEmail() => $"usuario-{Guid.NewGuid():N}@pruebas.aitodu.pe";
}
