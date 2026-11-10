namespace AiToDu.Platform.Shared.Domain.Model.ValueObjects;

public readonly record struct ActorId
{
    public Guid Value { get; }
    public ActorId(Guid value)
    {
        if (value == Guid.Empty) throw new ArgumentException("ActorId is required.");
        Value = value;
    }
    public override string ToString() => Value.ToString();
}
