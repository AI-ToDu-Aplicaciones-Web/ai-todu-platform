using AiToDu.Platform.Subscriptions.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiToDu.Platform.Subscriptions.Interfaces.Rest;

[ApiController, Authorize(Roles="BUSINESS_OWNER"), Route("api/v1/subscription")]
public sealed class SubscriptionsController(SubscriptionService subscriptions) : ControllerBase
{
    public sealed record Selection(string Plan);
    [HttpGet] public async Task<IActionResult> Current(CancellationToken ct) => Ok(await subscriptions.Current(ct));
    [HttpPut, PlanAllocation("plan")]
    public async Task<IActionResult> Select(Selection choice, CancellationToken ct) =>
        await subscriptions.Choose(choice.Plan, ct) ? Ok(await subscriptions.Current(ct)) :
        Problem(statusCode:409, detail:"Unknown plan or current usage exceeds its limits.");
}
