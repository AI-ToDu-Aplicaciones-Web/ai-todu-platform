using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.MySql;

namespace AiToDu.Platform.IntegrationTests.Infrastructure;

/// <summary>
/// Starts the real ASP.NET Core API against a throwaway MySQL database and applies both migration sets.
/// Set AITODU_TEST_MYSQL to an existing test database connection string to skip Docker (for example in CI).
/// </summary>
public sealed class AiToDuApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private MySqlContainer? _mysql;

    private static readonly TimeZoneInfo Lima = TimeZoneInfo.FindSystemTimeZoneById("America/Lima");

    public TestClock Clock { get; } = new();

    /// <summary>The warehouse business date, computed exactly like WarehouseClock.</summary>
    public DateOnly Today => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(Clock.GetUtcNow(), Lima).DateTime);

    public async Task InitializeAsync()
    {
        var connection = Environment.GetEnvironmentVariable("AITODU_TEST_MYSQL");
        if (string.IsNullOrWhiteSpace(connection))
        {
            _mysql = new MySqlBuilder().WithImage("mysql:8.4").Build();
            await _mysql.StartAsync();
            connection = _mysql.GetConnectionString().TrimEnd(';') + ";SslMode=Disabled;AllowPublicKeyRetrieval=True";
        }

        // Program.cs reads these values before Build(), so they are supplied as environment variables,
        // which WebApplication.CreateBuilder loads with priority over appsettings.json.
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", connection);
        Environment.SetEnvironmentVariable("Database__MigrateOnStartup", "true");
        Environment.SetEnvironmentVariable("Authentication__SigningKey", "integration-tests-signing-key-ai-todu-2026");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Not "Development": avoids loading a developer's user secrets (and their local database).
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Clock);
            services.AddSingleton<IStartupFilter, RandomRemoteIpStartupFilter>();
        });
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await DisposeAsync();
        if (_mysql is not null) await _mysql.DisposeAsync();
    }

    /// <summary>
    /// The authentication endpoints allow 20 requests per minute per IP. TestServer has no IP,
    /// so every request would share one bucket; a random address per request keeps tests independent.
    /// </summary>
    private sealed class RandomRemoteIpStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            app.Use(async (context, nextMiddleware) =>
            {
                context.Connection.RemoteIpAddress = new IPAddress(new byte[] { 10, (byte)Random.Shared.Next(256),
                    (byte)Random.Shared.Next(256), (byte)Random.Shared.Next(1, 255) });
                await nextMiddleware(context);
            });
            next(app);
        };
    }
}

[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<AiToDuApiFactory>
{
    public const string Name = "AI-ToDu API";
}