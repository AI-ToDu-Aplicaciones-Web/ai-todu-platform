using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.MySql;

namespace AiToDu.Platform.IntegrationTests.Infrastructure;

/// <summary>
/// Starts the real ASP.NET Core API against a throwaway MySQL database and applies both migration sets.
/// Set AITODU_TEST_MYSQL to an existing test database connection string to skip Docker (for example in CI).
/// </summary>
public sealed class AiToDuApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private MySqlContainer? _mysql;

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
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await DisposeAsync();
        if (_mysql is not null) await _mysql.DisposeAsync();
    }
}
