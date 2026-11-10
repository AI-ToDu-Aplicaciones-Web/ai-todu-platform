using AiToDu.Platform.Iam.Domain.Model.Aggregates;
namespace AiToDu.Platform.Iam.Application.Model;

public sealed record AuthenticatedUser(User User, string Token, DateTimeOffset ExpiresAt);
