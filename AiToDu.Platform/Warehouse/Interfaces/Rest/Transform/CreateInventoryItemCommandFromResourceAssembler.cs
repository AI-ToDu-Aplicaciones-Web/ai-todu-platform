using AiToDu.Platform.Warehouse.Domain.Model.Commands;
using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
using AiToDu.Platform.Warehouse.Interfaces.Rest.Resources;
namespace AiToDu.Platform.Warehouse.Interfaces.Rest.Transform;

public static class CreateInventoryItemCommandFromResourceAssembler
{ public static CreateInventoryItemCommand ToCommandFromResource(CreateInventoryItemResource r) => new(r.Sku, r.Name, r.Category, r.MinimumStock, r.UnitPrice); }
