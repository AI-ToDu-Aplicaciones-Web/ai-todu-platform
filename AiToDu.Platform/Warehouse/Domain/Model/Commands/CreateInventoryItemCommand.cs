using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
namespace AiToDu.Platform.Warehouse.Domain.Model.Commands;

public sealed record CreateInventoryItemCommand(string Sku, string Name, string Category, decimal MinimumStock, decimal UnitPrice);
