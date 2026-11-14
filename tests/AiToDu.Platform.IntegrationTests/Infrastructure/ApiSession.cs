using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AiToDu.Platform.Iam.Interfaces.Rest.Resources;
using AiToDu.Platform.Warehouse.Interfaces.Rest.Resources;

namespace AiToDu.Platform.IntegrationTests.Infrastructure;

/// <summary>An HTTP client signed in as the owner of a brand-new organization.</summary>
public sealed class ApiSession
{
    public const string InventoryUrl = "/api/v1/inventory-items";
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private ApiSession(HttpClient http) => Http = http;

    public HttpClient Http { get; }

    public static async Task<ApiSession> SignUpOwnerAsync(AiToDuApiFactory factory, string organizationName)
    {
        var http = factory.CreateClient();
        var email = $"owner-{Guid.NewGuid():N}@pruebas.aitodu.pe";
        const string password = "Clave-Segura-2026!";

        var signUp = await http.PostAsJsonAsync("/api/v1/authentication/sign-up",
            new SignUpResource(organizationName, "Dueño de prueba", email, password), Json);
        await ExpectStatus(signUp, HttpStatusCode.Created);

        var signIn = await http.PostAsJsonAsync("/api/v1/authentication/sign-in", new SignInResource(email, password), Json);
        var session = await Read<AuthenticatedUserResource>(signIn, HttpStatusCode.OK);
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.Token);
        return new ApiSession(http);
    }

    // ---------- Inventory ----------

    // ---------- Sales (only what the inventory tests need) ----------

    // ---------- Assertions ----------

    public static async Task<T> Read<T>(HttpResponseMessage response, HttpStatusCode expected)
    {
        await ExpectStatus(response, expected);
        var value = await response.Content.ReadFromJsonAsync<T>(Json);
        Assert.NotNull(value);
        return value!;
    }

    public static async Task ExpectStatus(HttpResponseMessage response, HttpStatusCode expected)
    {
        if (response.StatusCode == expected) return;
        var body = await response.Content.ReadAsStringAsync();
        Assert.Fail($"{response.RequestMessage?.Method} {response.RequestMessage?.RequestUri?.PathAndQuery}: " +
                    $"expected {(int)expected} {expected} but got {(int)response.StatusCode} {response.StatusCode}. Body: {body}");
    }
}