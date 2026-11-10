namespace AiToDu.Platform.Iam.Domain.Model.Commands;

public sealed record CreateOrganizationUserCommand(string FullName, string Email, string Password, string Role);
