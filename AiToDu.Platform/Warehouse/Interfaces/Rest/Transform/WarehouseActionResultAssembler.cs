using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using AiToDu.Platform.Shared.Application.Model;
using AiToDu.Platform.Warehouse.Domain.Model.Aggregates;
using AiToDu.Platform.Warehouse.Resources;
namespace AiToDu.Platform.Warehouse.Interfaces.Rest.Transform;

public static class WarehouseActionResultAssembler
{
    public static IActionResult ToActionResult(ControllerBase controller, Result<InventoryItem> result, DateOnly today, IStringLocalizer<WarehouseErrorMessages> messages, bool created = false)
    {
        if (!result.IsSuccess) return ToProblem(controller, result.Error!, messages);
        var resource = InventoryItemResourceFromEntityAssembler.ToResourceFromEntity(result.Value!, today);
        controller.Response.Headers.ETag = "\"" + resource.Version + "\"";
        return created ? controller.CreatedAtAction("GetById", new { id = resource.Id }, resource) : controller.Ok(resource);
    }
    public static IActionResult ToProblem(ControllerBase controller, Error error, IStringLocalizer<WarehouseErrorMessages> messages)
    {
        var status = error.Kind switch { ErrorKind.NotFound => 404, ErrorKind.Conflict => 409, ErrorKind.Forbidden => 403, _ => 400 };
        var detail = messages[error.Code];
        var problem = new ProblemDetails { Status = status, Title = messages["RequestRejected"], Detail = detail.ResourceNotFound ? messages["InvalidRequest"] : detail, Type = "urn:ai-todu:warehouse:" + error.Code, Instance = controller.Request.Path };
        problem.Extensions["code"] = error.Code;
        return new ObjectResult(problem) { StatusCode = status };
    }
}
