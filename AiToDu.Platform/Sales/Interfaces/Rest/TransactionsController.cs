using System.ComponentModel.DataAnnotations;
using AiToDu.Platform.Sales.Application;
using AiToDu.Platform.Sales.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiToDu.Platform.Sales.Interfaces.Rest;

[ApiController, Authorize(Policy = "sales.write")]
[Route("api/v1/transactions")]
public sealed class TransactionsController(SalesService sales, SalesRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) => Ok(await repository.Sales.AsNoTracking()
        .OrderByDescending(x => x.CreatedAt).ToListAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Checkout(Checkout resource, CancellationToken ct) => Ok(await sales.Checkout(resource, ct));

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
        var query = repository.Customers.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.Name.Contains(search.Trim()));
        return Ok(await query.OrderBy(x => x.Name).ToListAsync(ct));
    }

    [HttpPost("/api/v1/customers")]
    public async Task<IActionResult> Customer(CustomerResource resource, CancellationToken ct) =>
        Ok(await repository.RegisterCustomer(resource.Name, ct));
}
public sealed record CustomerResource([property:Required, StringLength(160, MinimumLength=1)] string Name);
