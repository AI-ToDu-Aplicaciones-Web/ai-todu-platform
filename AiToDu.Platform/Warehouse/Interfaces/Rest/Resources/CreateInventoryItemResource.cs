using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
namespace AiToDu.Platform.Warehouse.Interfaces.Rest.Resources;

public sealed record CreateInventoryItemResource(string Sku, string Name, string Category, decimal MinimumStock, decimal UnitPrice);
