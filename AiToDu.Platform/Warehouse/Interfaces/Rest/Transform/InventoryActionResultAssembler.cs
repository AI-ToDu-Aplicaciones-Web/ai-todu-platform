using AiToDu.Platform.Shared.Application.Model;
using AiToDu.Platform.Shared.Domain.Model.Errors;
using AiToDu.Platform.Warehouse.Domain.Model.Aggregates;
using AiToDu.Platform.Warehouse.Domain.Model.Errors;
using AiToDu.Platform.Warehouse.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using ProblemDetailsFactory = AiToDu.Platform.Shared.Interfaces.Rest.ProblemDetails.ProblemDetailsFactory;

namespace AiToDu.Platform.Warehouse.Interfaces.Rest.Transform;

/// <summary>Maps Warehouse outcomes through the guide's shared ProblemDetailsFactory.</summary>
public static class InventoryActionResultAssembler
{
    public static IActionResult ToActionResult(ControllerBase controller, Result<InventoryItem> result,
        DateOnly today, IStringLocalizer<WarehouseErrorMessages> messages, ProblemDetailsFactory factory, bool created = false)
    {
        if (result.IsFailure) return ToProblem(controller, result.DomainError, messages, factory, result.Error);
        var resource = InventoryItemResourceFromEntityAssembler.ToResourceFromEntity(result.Value!, today);
        controller.Response.Headers.ETag = $"\"{resource.Version}\"";
        return created ? controller.CreatedAtAction("GetById", new { id = resource.Id }, resource) : controller.Ok(resource);
    }

    public static IActionResult ToProblem(ControllerBase controller, Error error,
        IStringLocalizer<WarehouseErrorMessages> messages, ProblemDetailsFactory factory, Enum? errorEnum = null)
    {
        var key = error.Code.Split('.').Last();
        if (errorEnum is null && Enum.TryParse<WarehouseErrorCode>(key, out var warehouseError)) errorEnum = warehouseError;
        var status = error.Type switch
        {
            EErrorType.NotFound => StatusCodes.Status404NotFound,
            EErrorType.Conflict => StatusCodes.Status409Conflict,
            EErrorType.Forbidden => StatusCodes.Status403Forbidden,
            EErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            EErrorType.Unexpected => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status400BadRequest
        };
        var localized = messages[key];
        return factory.CreateProblemDetails(controller, status, errorEnum,
            localized.ResourceNotFound ? error.Message : localized.Value);
    }
}
