using AiToDu.Platform.Warehouse.Domain.Model.Errors;
using ProblemDetailsFactory = AiToDu.Platform.Shared.Interfaces.Rest.ProblemDetails.ProblemDetailsFactory;
using AiToDu.Platform.Warehouse.Application.OutboundServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using AiToDu.Platform.Shared.Application.Model;
using AiToDu.Platform.Warehouse.Application.CommandServices;
using AiToDu.Platform.Warehouse.Application.QueryServices;
using AiToDu.Platform.Warehouse.Domain.Model.Queries;
using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
using AiToDu.Platform.Warehouse.Interfaces.Rest.Resources;
using AiToDu.Platform.Warehouse.Interfaces.Rest.Transform;
using AiToDu.Platform.Warehouse.Resources;
namespace AiToDu.Platform.Warehouse.Interfaces.Rest;

[ApiController]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
[Route("api/v1/inventory-items")]
[Authorize(Roles = "BUSINESS_OWNER,WAREHOUSE_OPERATOR,SALES_CLERK")]
public sealed class InventoryItemsController(IInventoryItemCommandService commands, IInventoryItemQueryService queries,
    IWarehouseClock clock, IStringLocalizer<WarehouseErrorMessages> messages, ProblemDetailsFactory problemDetailsFactory) : ControllerBase
{
    private DateOnly Today => DateOnly.FromDateTime(clock.GetCurrentTime().DateTime);
    [HttpGet]
    [ProducesResponseType<InventoryPageResource>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        if (page < 1 || page > 1000000 || pageSize < 1 || pageSize > 100 || search?.Length > 160)
            return InventoryActionResultAssembler.ToProblem(this, WarehouseErrors.From(WarehouseErrorCode.InvalidPagination), messages, problemDetailsFactory);
        var result = await queries.Handle(new GetInventoryItemsQuery(search, page, pageSize), ct);
        return Ok(new InventoryPageResource(result.Items.Select(x => InventoryItemResourceFromEntityAssembler.ToResourceFromEntity(x, Today)).ToList(), result.Total, page, pageSize));
    }
    [HttpGet("by-sku/{sku}")]
    public async Task<IActionResult> BySku(string sku, CancellationToken ct)
    {
        try
        {
            var item = await queries.FindBySku(sku, ct);
            return item is null ? Problem(statusCode:404, detail:"Unknown SKU.") : Ok(InventoryItemResourceFromEntityAssembler.ToResourceFromEntity(item, Today));
        }
        catch (WarehouseDomainException) { return Problem(statusCode:400, detail:"Invalid SKU."); }
    }

    [HttpGet("{id:int:min(1)}")]
    [ProducesResponseType<InventoryItemResource>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var item = await queries.Handle(new GetInventoryItemByIdQuery(new InventoryItemId(id)), ct);
        if (item is null) return InventoryActionResultAssembler.ToProblem(this, WarehouseErrors.From(WarehouseErrorCode.ItemNotFound), messages, problemDetailsFactory);
        Response.Headers.ETag = "\"" + item.Version + "\"";
        return Ok(InventoryItemResourceFromEntityAssembler.ToResourceFromEntity(item, Today));
    }

    [AiToDu.Platform.Subscriptions.Interfaces.Rest.PlanAllocation("products")]
    [HttpPost]
    [ProducesResponseType<InventoryItemResource>(StatusCodes.Status201Created)]
    [Authorize(Policy = "inventory.write")]
    public async Task<IActionResult> Create([FromBody] CreateInventoryItemResource resource, CancellationToken ct) =>
        InventoryActionResultAssembler.ToActionResult(this, await commands.Handle(CreateInventoryItemCommandFromResourceAssembler.ToCommandFromResource(resource), ct), Today, messages, problemDetailsFactory, true);

    [HttpPost("{id:int:min(1)}/lots")]
    [ProducesResponseType<InventoryItemResource>(StatusCodes.Status200OK)]
    [Authorize(Policy = "inventory.write")]
    public async Task<IActionResult> Receive(int id, [FromBody] ReceiveStockLotResource resource, CancellationToken ct) =>
        InventoryActionResultAssembler.ToActionResult(this, await commands.Handle(ReceiveStockLotCommandFromResourceAssembler.ToCommandFromResource(id, resource), ct), Today, messages, problemDetailsFactory, false);

    [HttpPost("{id:int:min(1)}/movements")]
    [ProducesResponseType<InventoryItemResource>(StatusCodes.Status200OK)]
    [Authorize(Policy = "inventory.write")]
    public async Task<IActionResult> Move(int id, [FromBody] RecordInventoryMovementResource resource, CancellationToken ct) =>
        InventoryActionResultAssembler.ToActionResult(this, await commands.Handle(RecordInventoryMovementCommandFromResourceAssembler.ToCommandFromResource(id, resource), ct), Today, messages, problemDetailsFactory, false);

    [HttpPost("{id:int:min(1)}/counts")]
    [ProducesResponseType<InventoryItemResource>(StatusCodes.Status200OK)]
    [Authorize(Policy = "inventory.write")]
    public async Task<IActionResult> Count(int id, [FromBody] RegisterStockCountResource resource, CancellationToken ct) =>
        InventoryActionResultAssembler.ToActionResult(this, await commands.Handle(RegisterStockCountCommandFromResourceAssembler.ToCommandFromResource(id, resource), ct), Today, messages, problemDetailsFactory, false);

    [HttpPost("{id:int:min(1)}/returns")]
    [ProducesResponseType<InventoryItemResource>(StatusCodes.Status200OK)]
    [Authorize(Policy = "inventory.write")]
    public async Task<IActionResult> Return(int id, [FromBody] ClassifyInventoryReturnResource resource, [FromServices] AiToDu.Platform.Sales.Application.SalesService sales, CancellationToken ct)
    {
        await sales.ReturnByReceipt(id, resource.SourceLotId, resource.OperationId, resource.Reference, resource.Quantity, resource.Disposition.ToString(), resource.Reason, ct);
        return await GetById(id, ct);
    }

    [HttpPatch("{id:int:min(1)}/lots/{lotId:guid}/quarantine")]
    [ProducesResponseType<InventoryItemResource>(StatusCodes.Status200OK)]
    [Authorize(Policy = "inventory.write")]
    public async Task<IActionResult> Quarantine(int id, Guid lotId, [FromBody] ChangeLotQuarantineResource resource, CancellationToken ct) =>
        InventoryActionResultAssembler.ToActionResult(this, await commands.Handle(ChangeLotQuarantineCommandFromResourceAssembler.ToCommandFromResource(id, lotId, resource), ct), Today, messages, problemDetailsFactory, false);

    [HttpPatch("{id:int:min(1)}/lots/{lotId:guid}/location")]
    [ProducesResponseType<InventoryItemResource>(StatusCodes.Status200OK)]
    [Authorize(Policy = "inventory.write")]
    public async Task<IActionResult> Relocate(int id, Guid lotId, [FromBody] RelocateStockLotResource resource, CancellationToken ct) =>
        InventoryActionResultAssembler.ToActionResult(this, await commands.Handle(RelocateStockLotCommandFromResourceAssembler.ToCommandFromResource(id, lotId, resource), ct), Today, messages, problemDetailsFactory, false);
}
