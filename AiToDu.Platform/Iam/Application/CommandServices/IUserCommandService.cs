using AiToDu.Platform.Iam.Application.Model;
using AiToDu.Platform.Iam.Domain.Model.Aggregates;
using AiToDu.Platform.Iam.Domain.Model.Commands;
using AiToDu.Platform.Shared.Application.Model;
namespace AiToDu.Platform.Iam.Application.CommandServices;

public interface IUserCommandService
{
    Task<Result<User>> Handle(SignUpCommand command, CancellationToken ct);
    Task<Result<AuthenticatedUser>> Handle(SignInCommand command, CancellationToken ct);
    Task<Result<User>> Handle(CreateOrganizationUserCommand command, CancellationToken ct);
    Task<Result<User>> Handle(SetUserStatusCommand command, CancellationToken ct);
}
