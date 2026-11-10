using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
namespace AiToDu.Platform.Warehouse.Domain.Model.Queries;

public sealed record GetInventoryItemsQuery(string? Search = null, int Page = 1, int PageSize = 20);
