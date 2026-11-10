using System.Globalization;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using AiToDu.Platform.Shared.Domain.Repositories;
using AiToDu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using AiToDu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using AiToDu.Platform.Shared.Infrastructure.Security;
using AiToDu.Platform.Warehouse.Application.CommandServices;
using AiToDu.Platform.Warehouse.Application.QueryServices;
using AiToDu.Platform.Warehouse.Application.Internal.CommandServices;
using AiToDu.Platform.Warehouse.Application.Internal.QueryServices;
using AiToDu.Platform.Warehouse.Application.OutboundServices;
using AiToDu.Platform.Warehouse.Application.Acl;
using AiToDu.Platform.Warehouse.Interfaces.Acl;
using AiToDu.Platform.Warehouse.Domain.Repositories;
using AiToDu.Platform.Warehouse.Infrastructure.Identity;
using AiToDu.Platform.Warehouse.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using AiToDu.Platform.Warehouse.Domain.Model.Errors;

var builder = WebApplication.CreateBuilder(args);
var connection = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connection))
    connection = builder.Environment.IsDevelopment() ? "Host=127.0.0.1;Port=54339;Database=ai_todu;Username=postgres" : throw new InvalidOperationException("Configure ConnectionStrings__DefaultConnection.");
if (!builder.Environment.IsDevelopment() && (builder.Configuration["Authentication:SigningKey"]?.Length ?? 0) < 32)
    throw new InvalidOperationException("Configure a signing key with at least 32 characters.");
builder.Services.AddDbContext<AppDbContext>(o => o.UseNpgsql(connection, x => x.MigrationsHistoryTable("__ef_migrations_history", "warehouse")));
builder.Services.AddScoped<IInventoryItemRepository, InventoryItemRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IInventoryItemCommandService, InventoryItemCommandService>();
builder.Services.AddScoped<IInventoryItemQueryService, InventoryItemQueryService>();
builder.Services.AddScoped<IWarehouseContextFacade, WarehouseContextFacade>();
builder.Services.AddScoped<ICurrentWarehouseActor, HttpCurrentWarehouseActor>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IWarehouseClock, WarehouseClock>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication("Bearer").AddScheme<AuthenticationSchemeOptions, WarehouseAuthenticationHandler>("Bearer", _ => { });
builder.Services.AddAuthorization();
builder.Services.AddLocalization();
builder.Services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi(o => o.AddDocumentTransformer<AiToDu.Platform.Shared.Infrastructure.Interfaces.AspNetCore.OpenApi.BearerSecuritySchemeTransformer>());
builder.Services.AddProblemDetails();
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:5173", "http://127.0.0.1:5173"]).AllowAnyHeader().AllowAnyMethod().WithExposedHeaders("ETag")));
var app = builder.Build();
app.UseRequestLocalization(new RequestLocalizationOptions { DefaultRequestCulture = new RequestCulture("en-US"), SupportedCultures = [new CultureInfo("en-US"), new CultureInfo("es-419")], SupportedUICultures = [new CultureInfo("en"), new CultureInfo("es")] });
app.Use(async (context, next) =>
{
    try { await next(context); }
    catch (WarehouseDomainException e)
    {
        context.Response.StatusCode = e.Code.EndsWith("NotFound") ? 404 : e.IsConflict ? 409 : 400;
        await Results.Problem(statusCode: context.Response.StatusCode, title: "Inventory request rejected", extensions: new Dictionary<string, object?> { { "code", e.Code } }).ExecuteAsync(context);
    }
    catch (Exception e) when (e is not OperationCanceledException)
    {
        app.Logger.LogError(e, "Warehouse request failed");
        await Results.Problem(statusCode: 500, title: "The request could not be completed.", extensions: new Dictionary<string, object?> { { "code", "InternalError" } }).ExecuteAsync(context);
    }
});
app.UseCors();
app.UseAuthentication(); app.UseAuthorization();
app.MapControllers();
app.MapOpenApi();
app.MapScalarApiReference();
app.MapGet("/api/v1/health", async (AppDbContext db, CancellationToken ct) =>
    await db.Database.CanConnectAsync(ct) ? Results.Ok(new { status = "UP", service = "AI-ToDu Warehouse API" }) : Results.Problem(statusCode: 503, title: "Database unavailable"));
if (builder.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
}
app.Run();
public partial class Program { }
