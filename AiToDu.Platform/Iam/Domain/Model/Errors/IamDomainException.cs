namespace AiToDu.Platform.Iam.Domain.Model.Errors;

public sealed class IamDomainException(IamError code) : Exception(code.ToString())
{
    public IamError Code { get; } = code;
}
