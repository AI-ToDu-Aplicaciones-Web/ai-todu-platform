using System.Security.Claims;
using AiToDu.Platform.Iam.Domain.Model.Aggregates;
namespace AiToDu.Platform.Iam.Application.Internal.OutboundServices;

public interface ITokenService
{
    (string Token, DateTimeOffset ExpiresAt) GenerateToken(User user);
    ClaimsPrincipal? ValidateToken(string token);
}
