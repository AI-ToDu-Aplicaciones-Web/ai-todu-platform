using System.Security.Claims;
using AiToDu.Platform.Activity.Domain.Model;
using AiToDu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace AiToDu.Platform.Activity.Interfaces.Rest;

public sealed class AuditFilter(IServiceScopeFactory scopes) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var result = await next();
        var http = context.HttpContext;
        if (http.Request.Method is "GET" or "HEAD" || result.Exception != null ||
            (result.Result as IStatusCodeActionResult)?.StatusCode >= 400 ||
            !Guid.TryParse(http.User.FindFirstValue("sub"), out var actor) ||
            !Guid.TryParse(http.User.FindFirstValue("organization_id"), out var organization)) return;
        await using var scope = scopes.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var path = http.Request.Path.Value ?? "/";
        var module = path.StartsWith("/api/v1/transactions", StringComparison.OrdinalIgnoreCase) ||
                     path.StartsWith("/api/v1/customers", StringComparison.OrdinalIgnoreCase)
            ? "sales"
            : path.StartsWith("/api/v1/subscription", StringComparison.OrdinalIgnoreCase)
                ? "subscriptions"
                : path.Split('/', StringSplitOptions.RemoveEmptyEntries).Skip(2).FirstOrDefault() ?? "api";
        if (path.Length > 512) path = path[..512];
        db.Add(new AuditEntry(Guid.NewGuid(), organization, actor, module, http.Request.Method, path, DateTime.UtcNow));
        await db.SaveChangesAsync(CancellationToken.None);
    }
}
