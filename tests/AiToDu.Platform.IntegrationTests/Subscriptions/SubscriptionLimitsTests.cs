using System.Net;
using AiToDu.Platform.IntegrationTests.Infrastructure;

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

    private static string NewEmail() => $"usuario-{Guid.NewGuid():N}@pruebas.aitodu.pe";
}
