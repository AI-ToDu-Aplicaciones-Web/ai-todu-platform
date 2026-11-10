using System.Net.Mail;
using AiToDu.Platform.Iam.Domain.Model.Errors;
namespace AiToDu.Platform.Iam.Domain.Model.ValueObjects;

public sealed record EmailAddress
{
    public string Value { get; }
    public EmailAddress(string email)
    {
        var value = email?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(value) || value.Length > 254 ||
            !MailAddress.TryCreate(value, out var address) || address.Address != value)
            throw new IamDomainException(IamError.InvalidEmail);
        Value = value;
    }
}
