namespace AiToDu.Platform.Iam.Interfaces.Rest.Resources;

public sealed record UserResource(Guid Id, Guid OrganizationId, string OrganizationName,
    string FullName, string Email, string Role, bool IsActive, bool CanManageStock, bool CanSell);
