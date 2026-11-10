using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AiToDu.Platform.Warehouse.Application.OutboundServices;
namespace AiToDu.Platform.Warehouse.Interfaces.Rest;

[ApiController, Route("api/v1/warehouse/session"), Authorize(Roles = "BUSINESS_OWNER,WAREHOUSE_OPERATOR,SALES_CLERK")]
public sealed class WarehouseSessionController(ICurrentWarehouseActor actor) : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        organizationId = actor.OrganizationId.Value,
        actorId = actor.ActorId.Value,
        canManageInventory = actor.CanManageInventory,
        developmentIdentity = User.HasClaim("development_identity", "true")
    });
}
