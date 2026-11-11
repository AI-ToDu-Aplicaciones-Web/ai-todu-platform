using AiToDu.Platform.Subscriptions.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AiToDu.Platform.Subscriptions.Interfaces.Rest;

[AttributeUsage(AttributeTargets.Method)]
public sealed class PlanAllocationAttribute(string resource) : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var service = context.HttpContext.RequestServices.GetRequiredService<SubscriptionService>();
        var ct = context.HttpContext.RequestAborted;
        bool locked = await service.WithLock(async () => {
            var plan = await service.Current(ct);
            bool full = resource switch {
                "users" => plan.UserLimit is int users && await service.Users(ct) >= users,
                "products" => plan.SkuLimit is int skus && await service.Products(ct) >= skus,
                _ => false
            };
            if (full) context.Result = Conflict("Subscription resource limit reached.");
            else await next();
        }, ct);
        if (!locked) context.Result = Conflict("Organization is busy. Retry.");
    }
    private static ObjectResult Conflict(string detail) => new(new ProblemDetails { Status = 409, Detail = detail }) { StatusCode = 409 };
}
