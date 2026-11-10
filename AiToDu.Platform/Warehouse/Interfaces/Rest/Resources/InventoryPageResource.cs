namespace AiToDu.Platform.Warehouse.Interfaces.Rest.Resources;

public sealed record InventoryPageResource(IReadOnlyList<InventoryItemResource> Items, int Total, int Page, int PageSize);
