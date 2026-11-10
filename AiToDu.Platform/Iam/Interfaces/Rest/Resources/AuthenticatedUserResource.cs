namespace AiToDu.Platform.Iam.Interfaces.Rest.Resources;

public sealed record AuthenticatedUserResource(UserResource User, string Token, DateTimeOffset ExpiresAt);
