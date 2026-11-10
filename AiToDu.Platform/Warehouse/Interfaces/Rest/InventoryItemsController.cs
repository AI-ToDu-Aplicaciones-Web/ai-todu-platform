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
    IWarehouseClock clock, IStringLocalizer<WarehouseErrorMessages> messages) : ControllerBase
{
    private DateOnly Today => DateOnly.FromDateTime(clock.GetCurrentTime().DateTime);
    [HttpGet]
    [ProducesResponseType<InventoryPageResource>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await queries.Handle(new GetInventoryItemsQuery(search, page, pageSize), ct);
        return Ok(new InventoryPageResource(result.Items.Select(x => InventoryItemResourceFromEntityAssembler.ToResourceFromEntity(x, Today)).ToList(), result.Total, page, pageSize));
    }
    [HttpGet("{id:guid}")]
    [ProducesResponseType<InventoryItemResource>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var item = await queries.Handle(new GetInventoryItemByIdQuery(new InventoryItemId(id)), ct);
        if (item is null) return WarehouseActionResultAssembler.ToProblem(this, new Error("ItemNotFound", ErrorKind.NotFound), messages);
        Response.Headers.ETag = "\"" + item.Version + "\"";
        return Ok(InventoryItemResourceFromEntityAssembler.ToResourceFromEntity(item, Today));
    }

    [HttpPost]
    [ProducesResponseType<InventoryItemResource>(StatusCodes.Status201Created)]
    [Authorize(Roles = "BUSINESS_OWNER,WAREHOUSE_OPERATOR")]
    public async Task<IActionResult> Create([FromBody] CreateInventoryItemResource resource, CancellationToken ct) =>
        WarehouseActionResultAssembler.ToActionResult(this, await commands.Handle(CreateInventoryItemCommandFromResourceAssembler.ToCommandFromResource(resource), ct), Today, messages, true);

    [HttpPost("{id:guid}/lots")]
    [ProducesResponseType<InventoryItemResource>(StatusCodes.Status200OK)]
    [Authorize(Roles = "BUSINESS_OWNER,WAREHOUSE_OPERATOR")]
    public async Task<IActionResult> Receive(Guid id, [FromBody] ReceiveStockLotResource resource, CancellationToken ct) =>
        WarehouseActionResultAssembler.ToActionResult(this, await commands.Handle(ReceiveStockLotCommandFromResourceAssembler.ToCommandFromResource(id, resource), ct), Today, messages, false);

    [HttpPost("{id:guid}/movements")]
    [ProducesResponseType<InventoryItemResource>(StatusCodes.Status200OK)]
    [Authorize(Roles = "BUSINESS_OWNER,WAREHOUSE_OPERATOR")]
    public async Task<IActionResult> Move(Guid id, [FromBody] RecordInventoryMovementResource resource, CancellationToken ct) =>
        WarehouseActionResultAssembler.ToActionResult(this, await commands.Handle(RecordInventoryMovementCommandFromResourceAssembler.ToCommandFromResource(id, resource), ct), Today, messages, false);

    [HttpPost("{id:guid}/counts")]
    [ProducesResponseType<InventoryItemResource>(StatusCodes.Status200OK)]
    [Authorize(Roles = "BUSINESS_OWNER,WAREHOUSE_OPERATOR")]
    public async Task<IActionResult> Count(Guid id, [FromBody] RegisterStockCountResource resource, CancellationToken ct) =>
        WarehouseActionResultAssembler.ToActionResult(this, await commands.Handle(RegisterStockCountCommandFromResourceAssembler.ToCommandFromResource(id, resource), ct), Today, messages, false);

    [HttpPost("{id:guid}/returns")]
    [ProducesResponseType<InventoryItemResource>(StatusCodes.Status200OK)]
    [Authorize(Roles = "BUSINESS_OWNER,WAREHOUSE_OPERATOR")]
    public async Task<IActionResult> Return(Guid id, [FromBody] ClassifyInventoryReturnResource resource, CancellationToken ct) =>
        WarehouseActionResultAssembler.ToActionResult(this, await commands.Handle(ClassifyInventoryReturnCommandFromResourceAssembler.ToCommandFromResource(id, resource), ct), Today, messages, false);

    [HttpPatch("{id:guid}/lots/{lotId:guid}/quarantine")]
    [ProducesResponseType<InventoryItemResource>(StatusCodes.Status200OK)]
    [Authorize(Roles = "BUSINESS_OWNER,WAREHOUSE_OPERATOR")]
    public async Task<IActionResult> Quarantine(Guid id, Guid lotId, [FromBody] ChangeLotQuarantineResource resource, CancellationToken ct) =>
        WarehouseActionResultAssembler.ToActionResult(this, await commands.Handle(ChangeLotQuarantineCommandFromResourceAssembler.ToCommandFromResource(id, lotId, resource), ct), Today, messages, false);

    [HttpPatch("{id:guid}/lots/{lotId:guid}/location")]
    [ProducesResponseType<InventoryItemResource>(StatusCodes.Status200OK)]
    [Authorize(Roles = "BUSINESS_OWNER,WAREHOUSE_OPERATOR")]
    public async Task<IActionResult> Relocate(Guid id, Guid lotId, [FromBody] RelocateStockLotResource resource, CancellationToken ct) =>
        WarehouseActionResultAssembler.ToActionResult(this, await commands.Handle(RelocateStockLotCommandFromResourceAssembler.ToCommandFromResource(id, lotId, resource), ct), Today, messages, false);
}
