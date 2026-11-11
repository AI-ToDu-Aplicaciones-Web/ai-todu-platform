using System.Security.Claims;
using System.Text.Encodings.Web;
using AiToDu.Platform.Iam.Application.Internal.OutboundServices;
using AiToDu.Platform.Iam.Domain.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
namespace AiToDu.Platform.Iam.Infrastructure.Identity;

public sealed class IamAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger,
    UrlEncoder encoder, ITokenService tokens, IUserRepository users)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var header = Request.Headers.Authorization.ToString();
        if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) return AuthenticateResult.NoResult();
        var principal = tokens.ValidateToken(header[7..]);
        if (principal is null || !Guid.TryParse(principal.FindFirstValue("sub"), out var publicId) ||
            !Guid.TryParse(principal.FindFirstValue("organization_id"), out var organizationId))
            return AuthenticateResult.Fail("Invalid credentials.");
        var user = await users.FindByPublicIdAsync(publicId, Context.RequestAborted);
        if (user is null || !user.IsActive || user.OrganizationId != organizationId || !principal.IsInRole(user.Role))
            return AuthenticateResult.Fail("The account is unavailable.");
        var identity = (ClaimsIdentity)principal.Identity!;
        if (user.CanManageStock) identity.AddClaim(new Claim("permission", "inventory.write"));
        if (user.CanSell) identity.AddClaim(new Claim("permission", "sales.write"));
        return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
    }
}
