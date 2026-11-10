using AiToDu.Platform.Iam.Domain.Model.Errors;
using AiToDu.Platform.Iam.Domain.Model.ValueObjects;
using AiToDu.Platform.Shared.Domain.Model.Aggregates;
using AiToDu.Platform.Shared.Domain.Model.Entities;
namespace AiToDu.Platform.Iam.Domain.Model.Aggregates;

public sealed class User : AggregateRoot, IAuditableEntity
{
    public UserId Id { get; private set; }
    public Guid PublicId { get; private set; }
    public Guid OrganizationId { get; private set; }
    public string OrganizationName { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public EmailAddress Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Role { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    private User() { }

    private User(Guid organizationId, string organizationName, string fullName,
        EmailAddress email, string passwordHash, string role)
    {
        if (organizationId == Guid.Empty || string.IsNullOrWhiteSpace(organizationName) ||
            organizationName.Trim().Length > 160 || string.IsNullOrWhiteSpace(fullName) ||
            fullName.Trim().Length > 160 || string.IsNullOrWhiteSpace(passwordHash))
            throw new IamDomainException(IamError.RequiredFields);
        PublicId = Guid.NewGuid();
        OrganizationId = organizationId;
        OrganizationName = organizationName.Trim();
        FullName = fullName.Trim();
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        IsActive = true;
    }

    public static User RegisterOwner(string organizationName, string fullName, EmailAddress email, string passwordHash) =>
        new(Guid.NewGuid(), organizationName, fullName, email, passwordHash, UserRole.Owner);

    public static User RegisterStaff(Guid organizationId, string organizationName, string fullName,
        EmailAddress email, string passwordHash, string role)
    {
        UserRole.ValidateStaffRole(role);
        return new User(organizationId, organizationName, fullName, email, passwordHash, role);
    }

    public void SetActive(bool active) => IsActive = active;
}
