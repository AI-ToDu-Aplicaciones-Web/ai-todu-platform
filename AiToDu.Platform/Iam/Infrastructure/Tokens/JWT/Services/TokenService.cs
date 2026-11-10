using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AiToDu.Platform.Iam.Application.Internal.OutboundServices;
using AiToDu.Platform.Iam.Domain.Model.Aggregates;
using Microsoft.IdentityModel.Tokens;
namespace AiToDu.Platform.Iam.Infrastructure.Tokens.JWT.Services;

public sealed class TokenService(IConfiguration configuration, TimeProvider clock) : ITokenService
{
    private SymmetricSecurityKey Key => new(Encoding.UTF8.GetBytes(configuration["Authentication:SigningKey"]
        ?? throw new InvalidOperationException("Configure Authentication:SigningKey.")));

    public (string Token, DateTimeOffset ExpiresAt) GenerateToken(User user)
    {
        var now = clock.GetUtcNow();
        var expiresAt = now.AddMinutes(60);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.PublicId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("organization_id", user.OrganizationId.ToString()),
            new Claim("role", user.Role)
        };
        var token = new JwtSecurityToken(configuration["Authentication:Issuer"], configuration["Authentication:Audience"],
            claims, now.UtcDateTime, expiresAt.UtcDateTime, new SigningCredentials(Key, SecurityAlgorithms.HmacSha256));
        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            return new JwtSecurityTokenHandler { MapInboundClaims = false }.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true, ValidIssuer = configuration["Authentication:Issuer"],
                ValidateAudience = true, ValidAudience = configuration["Authentication:Audience"],
                ValidateLifetime = true, RequireExpirationTime = true, RequireSignedTokens = true,
                ValidateIssuerSigningKey = true, IssuerSigningKey = Key,
                ValidAlgorithms = [SecurityAlgorithms.HmacSha256], ClockSkew = TimeSpan.FromSeconds(15),
                NameClaimType = "sub", RoleClaimType = "role"
            }, out _);
        }
        catch (Exception e) when (e is SecurityTokenException or ArgumentException) { return null; }
    }
}
