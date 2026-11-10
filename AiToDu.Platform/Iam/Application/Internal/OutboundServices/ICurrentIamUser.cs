namespace AiToDu.Platform.Iam.Application.Internal.OutboundServices;

public interface ICurrentIamUser
{
    Guid PublicId { get; }
    Guid OrganizationId { get; }
    bool IsOwner { get; }
}
