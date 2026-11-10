using System.Security.Claims;
using AiToDu.Platform.Iam.Application.Internal.OutboundServices;
using AiToDu.Platform.Iam.Domain.Model.ValueObjects;
namespace AiToDu.Platform.Iam.Infrastructure.Identity;

public sealed class HttpCurrentIamUser(IHttpContextAccessor accessor) : ICurrentIamUser
{
    private ClaimsPrincipal User => accessor.HttpContext?.User ?? throw new InvalidOperationException("An authenticated user is required.");
    public Guid PublicId => Guid.Parse(User.FindFirstValue("sub")!);
    public Guid OrganizationId => Guid.Parse(User.FindFirstValue("organization_id")!);
    public bool IsOwner => User.IsInRole(UserRole.Owner);
}
