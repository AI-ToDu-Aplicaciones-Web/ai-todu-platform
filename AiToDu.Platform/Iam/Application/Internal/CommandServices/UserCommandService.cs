using AiToDu.Platform.Iam.Application.CommandServices;
using AiToDu.Platform.Iam.Application.Exceptions;
using AiToDu.Platform.Iam.Application.Internal.OutboundServices;
using AiToDu.Platform.Iam.Application.Model;
using AiToDu.Platform.Iam.Domain.Model.Aggregates;
using AiToDu.Platform.Iam.Domain.Model.Commands;
using AiToDu.Platform.Iam.Domain.Model.Errors;
using AiToDu.Platform.Iam.Domain.Model.ValueObjects;
using AiToDu.Platform.Iam.Domain.Repositories;
using AiToDu.Platform.Shared.Application.Model;
using AiToDu.Platform.Shared.Domain.Repositories;
namespace AiToDu.Platform.Iam.Application.Internal.CommandServices;

public sealed class UserCommandService(IUserRepository repository, IHashingService hashingService,
    ITokenService tokenService, IUnitOfWork unitOfWork, ICurrentIamUser currentUser) : IUserCommandService
{
    public Task<Result<User>> Handle(SignUpCommand c, CancellationToken ct) => Register(c.Email, c.Password,
        (email, hash) => User.RegisterOwner(c.OrganizationName, c.FullName, email, hash), ct);

    public async Task<Result<User>> Handle(CreateOrganizationUserCommand c, CancellationToken ct)
    {
        if (!currentUser.IsOwner) return Failure<User>(IamError.Forbidden);
        var owner = await repository.FindByPublicIdAsync(currentUser.PublicId, ct);
        if (owner is null || !owner.IsActive || owner.OrganizationId != currentUser.OrganizationId)
            return Failure<User>(IamError.Forbidden);
        return await Register(c.Email, c.Password, (email, hash) =>
            User.RegisterStaff(owner.OrganizationId, owner.OrganizationName, c.FullName, email, hash, c.Role), ct);
    }

    private async Task<Result<User>> Register(string email, string password,
        Func<EmailAddress, string, User> create, CancellationToken ct)
    {
        try
        {
            var address = new EmailAddress(email);
            PasswordPolicy.Validate(password);
            if (await repository.FindByEmailAsync(address, ct) is not null) return Failure<User>(IamError.DuplicateEmail);
            var user = create(address, hashingService.HashPassword(password));
            await repository.AddAsync(user, ct);
            await unitOfWork.CompleteAsync(ct);
            return Result<User>.Success(user);
        }
        catch (IamDomainException e) { return Failure<User>(e.Code); }
        catch (DuplicateEmailException) { return Failure<User>(IamError.DuplicateEmail); }
    }

    public async Task<Result<AuthenticatedUser>> Handle(SignInCommand c, CancellationToken ct)
    {
        try
        {
            // The same invalid-credentials response is used for absent users and wrong passwords.
            var user = await repository.FindByEmailAsync(new EmailAddress(c.Email), ct);
            if (string.IsNullOrEmpty(c.Password) || System.Text.Encoding.UTF8.GetByteCount(c.Password) > 72 ||
                user is null || !hashingService.VerifyPassword(c.Password, user.PasswordHash))
                return Failure<AuthenticatedUser>(IamError.InvalidCredentials);
            if (!user.IsActive) return Failure<AuthenticatedUser>(IamError.AccountDisabled);
            var (token, expiresAt) = tokenService.GenerateToken(user);
            return Result<AuthenticatedUser>.Success(new AuthenticatedUser(user, token, expiresAt));
        }
        catch (IamDomainException) { return Failure<AuthenticatedUser>(IamError.InvalidCredentials); }
    }

    public async Task<Result<User>> SetAccess(Guid id, string role, bool? inventory, bool? sales, CancellationToken ct)
    {
        if (!currentUser.IsOwner) return Failure<User>(IamError.Forbidden);
        var user = await repository.FindByPublicIdAsync(id, ct);
        if (user == null || user.OrganizationId != currentUser.OrganizationId) return Failure<User>(IamError.UserNotFound);
        if (user.Role == UserRole.Owner) return Failure<User>(IamError.Forbidden);
        try { user.SetAccess(role, inventory, sales); }
        catch (IamDomainException error) { return Failure<User>(error.Code); }
        await unitOfWork.CompleteAsync(ct);
        return Result<User>.Success(user);
    }

    public async Task<Result<User>> Handle(SetUserStatusCommand c, CancellationToken ct)
    {
        if (!currentUser.IsOwner) return Failure<User>(IamError.Forbidden);
        var user = await repository.FindByPublicIdAsync(c.PublicId, ct);
        if (user is null || user.OrganizationId != currentUser.OrganizationId) return Failure<User>(IamError.UserNotFound);
        if (user.PublicId == currentUser.PublicId && !c.IsActive) return Failure<User>(IamError.CannotDisableSelf);
        user.SetActive(c.IsActive);
        await unitOfWork.CompleteAsync(ct);
        return Result<User>.Success(user);
    }

    private static Result<T> Failure<T>(IamError error) => Result<T>.Failure(IamErrors.From(error), error);
}
