using AiToDu.Platform.Shared.Domain.Model.ValueObjects;
namespace AiToDu.Platform.Iam.Domain.Model.ValueObjects;

public readonly record struct UserId(int Value) : IEntityId<int>;
