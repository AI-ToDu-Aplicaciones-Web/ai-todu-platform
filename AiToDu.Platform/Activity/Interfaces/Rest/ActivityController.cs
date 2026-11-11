using AiToDu.Platform.Activity.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiToDu.Platform.Activity.Interfaces.Rest;

[ApiController, Authorize, Route("api/v1")]
public sealed class ActivityController(IActivityService activity) : ControllerBase
{
    [HttpGet("notifications")] public async Task<IActionResult> Alerts(CancellationToken ct) => Ok(await activity.Alerts(ct));
    [HttpPut("notifications/{key}/read")] public async Task<IActionResult> Read(string key, CancellationToken ct) =>
        await activity.Read(key, ct) ? NoContent() : NotFound();
    [Authorize(Roles="BUSINESS_OWNER"), HttpGet("audit")] public async Task<IActionResult> History(CancellationToken ct) => Ok(await activity.History(ct));
}
