using System.Security.Claims;
using AiToDu.Platform.Warehouse.Application.OutboundServices;
using AiToDu.Platform.Shared.Domain.Model.ValueObjects;
namespace AiToDu.Platform.Warehouse.Infrastructure.Identity;

public sealed class HttpCurrentWarehouseActor(IHttpContextAccessor accessor) : ICurrentWarehouseActor
{
    private ClaimsPrincipal User => accessor.HttpContext?.User ?? throw new InvalidOperationException("A current identity is required.");
    public OrganizationId OrganizationId => new(Guid.Parse(User.FindFirstValue("organization_id")!));
    public ActorId ActorId => new(Guid.Parse(User.FindFirstValue("sub")!));
    public bool CanManageInventory => User.IsInRole("BUSINESS_OWNER") || User.IsInRole("WAREHOUSE_OPERATOR");
    public bool CanRecordSales => User.IsInRole("BUSINESS_OWNER") || User.IsInRole("SALES_CLERK");
}
