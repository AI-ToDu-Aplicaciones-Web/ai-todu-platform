using System.Threading.RateLimiting;
using AiToDu.Platform.Iam.Application.CommandServices;
using AiToDu.Platform.Iam.Application.Internal.CommandServices;
using AiToDu.Platform.Iam.Application.Internal.OutboundServices;
using AiToDu.Platform.Iam.Application.Internal.QueryServices;
using AiToDu.Platform.Iam.Application.QueryServices;
using AiToDu.Platform.Iam.Domain.Repositories;
using AiToDu.Platform.Iam.Infrastructure.Hashing.BCrypt.Services;
using AiToDu.Platform.Iam.Infrastructure.Identity;
using AiToDu.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using AiToDu.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using AiToDu.Platform.Iam.Infrastructure.Tokens.JWT.Services;
using Microsoft.AspNetCore.RateLimiting;
using System.Globalization;
using System.Text.Json.Serialization;
using AiToDu.Platform.Resources.Errors;
using AiToDu.Platform.Resources.Shared;
using AiToDu.Platform.Shared.Domain.Repositories;
using AiToDu.Platform.Shared.Infrastructure.Interfaces.AspNetCore.Configuration;
using AiToDu.Platform.Shared.Infrastructure.Interfaces.AspNetCore.OpenApi;
using AiToDu.Platform.Shared.Infrastructure.Mediator.Cortex.Configuration;
using AiToDu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using AiToDu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Interceptors;
using AiToDu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using AiToDu.Platform.Shared.Infrastructure.Pipeline.Middleware.Components;
using AiToDu.Platform.Warehouse.Application.Acl;
using AiToDu.Platform.Warehouse.Application.CommandServices;
using AiToDu.Platform.Warehouse.Application.Internal.CommandServices;
using AiToDu.Platform.Warehouse.Application.Internal.QueryServices;
using AiToDu.Platform.Warehouse.Application.OutboundServices;
using AiToDu.Platform.Warehouse.Application.QueryServices;
using AiToDu.Platform.Warehouse.Domain.Repositories;
using AiToDu.Platform.Warehouse.Infrastructure.Identity;
using AiToDu.Platform.Warehouse.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using AiToDu.Platform.Warehouse.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using AiToDu.Platform.Warehouse.Interfaces.Acl;
using Cortex.Mediator.Commands;
using Cortex.Mediator.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Scalar.AspNetCore;
using ProblemDetailsFactory = AiToDu.Platform.Shared.Interfaces.Rest.ProblemDetails.ProblemDetailsFactory;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddScoped<AiToDu.Platform.Activity.Application.IActivityService, AiToDu.Platform.Activity.Infrastructure.ActivityService>();
builder.Services.AddScoped<AiToDu.Platform.Subscriptions.Application.ISubscriptionService, AiToDu.Platform.Subscriptions.Infrastructure.SubscriptionService>();
builder.Services.AddControllers(options => {
    options.Conventions.Add(new KebabCaseRouteNamingConvention());
    options.Filters.Add<AiToDu.Platform.Activity.Interfaces.Rest.AuditFilter>();
})
    .AddDataAnnotationsLocalization()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddProblemDetails();
builder.Services.AddScoped<AiToDu.Platform.Sales.Infrastructure.Persistence.SalesRepository>();
builder.Services.AddScoped<AiToDu.Platform.Sales.Application.ISalesRepository>(sp => sp.GetRequiredService<AiToDu.Platform.Sales.Infrastructure.Persistence.SalesRepository>());
builder.Services.AddScoped<AiToDu.Platform.Sales.Application.ISalesDirectory>(sp => sp.GetRequiredService<AiToDu.Platform.Sales.Infrastructure.Persistence.SalesRepository>());
builder.Services.AddScoped<IAdjustmentReasons, AdjustmentReasons>();
builder.Services.AddScoped<AiToDu.Platform.Sales.Application.SalesService>();
builder.Services.AddExceptionHandler<AiToDu.Platform.Sales.Interfaces.Rest.SalesErrorHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddLocalization();
builder.Services.AddSingleton<IStringLocalizer<ErrorMessages>, StringLocalizer<ErrorMessages>>();
builder.Services.AddSingleton<IStringLocalizer<CommonMessages>, StringLocalizer<CommonMessages>>();
builder.Services.AddSingleton<ProblemDetailsFactory>();

builder.Services.AddDbContext<WarehouseDbContext>((services, options) =>
{
    var configured = builder.Configuration.GetConnectionString("DefaultConnection");
    var connection = !string.IsNullOrWhiteSpace(configured) ? Environment.ExpandEnvironmentVariables(configured)
        : builder.Environment.IsDevelopment() ? "Server=127.0.0.1;Port=3306;Database=ai_todu;User ID=ai_todu"
        : throw new InvalidOperationException("Database connection string is not set in the configuration.");
    options.UseMySQL(connection, database => database.MigrationsHistoryTable("__ef_warehouse_migrations"))
        .UseLoggerFactory(services.GetRequiredService<ILoggerFactory>())
        .AddInterceptors(services.GetRequiredService<DispatchDomainEventsInterceptor>())
        .EnableDetailedErrors();
});
// A Warehouse-specific derived context supplies mappings and tenant isolation;
// Shared's AppDbContext, repositories, interceptors and UnitOfWork are unchanged.
builder.Services.AddScoped<AppDbContext>(services => services.GetRequiredService<WarehouseDbContext>());
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<DispatchDomainEventsInterceptor>();
builder.Services.AddScoped(typeof(ICommandPipelineBehavior<>), typeof(LoggingCommandBehavior<>));
builder.Services.AddScoped(typeof(ICommandPipelineBehavior<>), typeof(ExceptionHandlingCommandBehavior<>));
builder.Services.AddScoped(typeof(ICommandPipelineBehavior<>), typeof(TransactionCommandBehavior<>));
builder.Services.AddCortexMediator([typeof(Program)]);

builder.Services.AddDbContext<IamDbContext>(options =>
{
    var connection = builder.Configuration.GetConnectionString("DefaultConnection") ??
        (builder.Environment.IsDevelopment() ? "Server=127.0.0.1;Port=3306;Database=ai_todu;User ID=ai_todu"
        : throw new InvalidOperationException("Configure the database connection."));
    options.UseMySQL(Environment.ExpandEnvironmentVariables(connection), database => database.MigrationsHistoryTable("__ef_iam_migrations"));
});
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IHashingService, HashingService>();
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddScoped<ICurrentIamUser, HttpCurrentIamUser>();
// Each context uses the original Shared UnitOfWork with its own DbContext.
builder.Services.AddScoped<IUserCommandService>(services => new UserCommandService(
    services.GetRequiredService<IUserRepository>(), services.GetRequiredService<IHashingService>(),
    services.GetRequiredService<ITokenService>(), new UnitOfWork(services.GetRequiredService<IamDbContext>()),
    services.GetRequiredService<ICurrentIamUser>()));
builder.Services.AddScoped<IUserQueryService, UserQueryService>();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("authentication", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 20, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});

builder.Services.AddScoped<IInventoryItemRepository, InventoryItemRepository>();
builder.Services.AddScoped<IInventoryItemCommandService, InventoryItemCommandService>();
builder.Services.AddScoped<IInventoryItemQueryService, InventoryItemQueryService>();
builder.Services.AddScoped<IWarehouseContextFacade, WarehouseContextFacade>();
builder.Services.AddScoped<ICurrentWarehouseActor, HttpCurrentWarehouseActor>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IWarehouseClock, WarehouseClock>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication("Bearer").AddScheme<AuthenticationSchemeOptions, IamAuthenticationHandler>("Bearer", _ => { });
builder.Services.AddAuthorization(options => {
    options.AddPolicy("inventory.write", policy => policy.RequireClaim("permission", "inventory.write"));
    options.AddPolicy("sales.write", policy => policy.RequireClaim("permission", "sales.write"));
});
builder.Services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:5173", "http://127.0.0.1:5173"])
    .AllowAnyHeader().AllowAnyMethod().WithExposedHeaders("ETag")));
if ((builder.Configuration["Authentication:SigningKey"]?.Length ?? 0) < 32)
    throw new InvalidOperationException("Configure a signing key with at least 32 characters.");

var app = builder.Build();
app.UseExceptionHandler();
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-US"),
    SupportedCultures = [new CultureInfo("en-US"), new CultureInfo("es-419")],
    SupportedUICultures = [new CultureInfo("en"), new CultureInfo("es")]
});
app.UseStatusCodePages();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
app.MapGet("/api/v1/health", async (AppDbContext database, CancellationToken ct) =>
    await database.Database.CanConnectAsync(ct) ? Results.Ok(new { status = "UP", service = "AI-ToDu Warehouse API" })
        : Results.Problem(statusCode: 503, title: "Database unavailable"));
if (builder.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<IamDbContext>().Database.MigrateAsync();
}
app.Run();
public partial class Program { }
