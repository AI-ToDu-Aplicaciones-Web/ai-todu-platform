using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
namespace AiToDu.Platform.Shared.Infrastructure.Security;

public sealed class WarehouseAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger,
    UrlEncoder encoder, IConfiguration configuration, IHostEnvironment environment)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var header = Request.Headers.Authorization.ToString();
        if (string.IsNullOrEmpty(header) && environment.IsDevelopment() && configuration.GetValue<bool>("Warehouse:AllowDevelopmentIdentity"))
        {
            var claims = new[] { new Claim("sub","22222222-2222-4222-8222-222222222222"),
                new Claim("organization_id","11111111-1111-4111-8111-111111111111"),new Claim("role","WAREHOUSE_OPERATOR"),new Claim("development_identity","true") };
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name, "sub", "role")), Scheme.Name)));
        }
        if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) return Task.FromResult(AuthenticateResult.NoResult());
        try
        {
            var key = configuration["Authentication:SigningKey"];
            if (string.IsNullOrEmpty(key) || key.Length < 32) return Task.FromResult(AuthenticateResult.Fail("Signing key is not configured."));
            var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
            var user = handler.ValidateToken(header[7..], new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = configuration["Authentication:Issuer"],
                ValidateAudience = true,
                ValidAudience = configuration["Authentication:Audience"],
                ValidateLifetime = true,
                RequireExpirationTime = true,
                RequireSignedTokens = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                ClockSkew = TimeSpan.FromSeconds(30),
                NameClaimType = "sub",
                RoleClaimType = "role"
            }, out _);
            if (!Guid.TryParse(user.FindFirstValue("sub"), out var actor) || actor == Guid.Empty ||
                !Guid.TryParse(user.FindFirstValue("organization_id"), out var organization) || organization == Guid.Empty)
                return Task.FromResult(AuthenticateResult.Fail("Identity claims are invalid."));
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(user, Scheme.Name)));
        }
        catch (Exception e) when (e is SecurityTokenException or ArgumentException)
        { return Task.FromResult(AuthenticateResult.Fail("Invalid bearer token.")); }
    }
}
