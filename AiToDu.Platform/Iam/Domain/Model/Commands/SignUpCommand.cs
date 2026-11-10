namespace AiToDu.Platform.Iam.Domain.Model.Commands;

public sealed record SignUpCommand(string OrganizationName, string FullName, string Email, string Password);
