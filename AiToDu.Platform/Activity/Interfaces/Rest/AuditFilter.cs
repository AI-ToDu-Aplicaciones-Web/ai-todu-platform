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
        db.Add(new AuditEntry(Guid.NewGuid(), organization, actor, http.Request.Method, http.Request.Path, DateTime.UtcNow));
        await db.SaveChangesAsync(CancellationToken.None);
    }
}
