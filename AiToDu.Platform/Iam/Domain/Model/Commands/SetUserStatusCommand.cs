namespace AiToDu.Platform.Iam.Domain.Model.Commands;

public sealed record SetUserStatusCommand(Guid PublicId, bool IsActive);
