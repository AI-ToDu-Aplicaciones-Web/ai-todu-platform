using System.ComponentModel.DataAnnotations;
using AiToDu.Platform.Sales.Application;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace AiToDu.Platform.Sales.Interfaces.Rest;

[ApiController, Authorize(Policy = "sales.write")]
[Route("api/v1/transactions")]
public sealed class TransactionsController(SalesService sales, ISalesDirectory repository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(DateOnly? from, DateOnly? to, CancellationToken ct)
    {
        if (from > to || to == DateOnly.MaxValue) return Problem(statusCode:400, detail:"Invalid date range.");
        DateTime? start = null, end = null;
        var zone = TimeZoneInfo.FindSystemTimeZoneById("America/Lima");
        if (from.HasValue)
        {
            start = TimeZoneInfo.ConvertTimeToUtc(from.Value.ToDateTime(TimeOnly.MinValue), zone);
        }
        if (to.HasValue)
        {
            end = TimeZoneInfo.ConvertTimeToUtc(to.Value.AddDays(1).ToDateTime(TimeOnly.MinValue), zone);
        }
        return Ok(await repository.History(start, end, ct));
    }
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Find(Guid id, CancellationToken ct) =>
        await repository.FindSale(id, ct) is { } sale ? Ok(sale) : Problem(statusCode:404, detail:"Unknown sale.");

    [HttpPost]
    public async Task<IActionResult> Checkout(Checkout resource, CancellationToken ct) => Ok(await sales.Checkout(resource, ct));

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct) => Ok(await sales.Cancel(id, ct));

    public sealed record Payment(string Reference);
    [HttpPost("{id:guid}/payment")]
    public async Task<IActionResult> Pay(Guid id, Payment payment, CancellationToken ct) => Ok(await sales.Pay(id, payment.Reference, ct));

    [HttpPost("{id:guid}/returns")]
    public async Task<IActionResult> Return(Guid id, ReturnRequest resource, CancellationToken ct) => Ok(await sales.Return(id, resource, ct));

    [HttpGet("{id:guid}/receipt")]
    public async Task<IActionResult> Receipt(Guid id, CancellationToken ct)
    {
        var sale = await repository.FindSale(id, ct);
        if (sale == null) return NotFound();
        if (sale.Receipt == null) return Problem(statusCode:409, detail:"Payment is pending.");
        return Ok(new { sale.Receipt, sale.CustomerName, sale.CreatedAt, sale.PaymentReference, sale.Lines, sale.Total });
    }

    [HttpGet("/api/v1/customers")]
    public async Task<IActionResult> Customers([FromQuery] string? search, CancellationToken ct)
    {
        return Ok(await repository.CustomersMatching(search, ct));
    }

    [HttpPost("/api/v1/customers")]
    public async Task<IActionResult> Customer(CustomerResource resource, CancellationToken ct) =>
        Ok(await repository.RegisterCustomer(resource.Name, ct));
}
public sealed record CustomerResource([Required, StringLength(160, MinimumLength=1)] string Name);
