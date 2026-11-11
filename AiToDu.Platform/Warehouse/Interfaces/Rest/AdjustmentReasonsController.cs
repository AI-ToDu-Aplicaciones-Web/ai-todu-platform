using System.ComponentModel.DataAnnotations;
using AiToDu.Platform.Warehouse.Application.CommandServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiToDu.Platform.Warehouse.Interfaces.Rest;

[ApiController, Authorize, Route("api/v1/adjustment-reasons")]
public sealed class AdjustmentReasonsController(IAdjustmentReasons reasons) : ControllerBase
{
    public sealed record NewReason([Required, StringLength(120)] string Name);
    [HttpGet] public async Task<IActionResult> List(CancellationToken ct) => Ok(await reasons.List(ct));
    [HttpPost, Authorize(Roles="BUSINESS_OWNER")]
    public async Task<IActionResult> Add(NewReason input, CancellationToken ct) =>
        await reasons.Add(input.Name, ct) is { } reason ? Ok(reason) : Problem(statusCode:409, detail:"Reason already exists.");
}
