namespace AiToDu.Platform.IntegrationTests.Infrastructure;

/// <summary>
/// Real clock that can be moved forward so a test can let a lot expire.
/// Tokens are issued before moving it, and JWT lifetime is validated against real time.
/// </summary>
public sealed class TestClock : TimeProvider
{
    private TimeSpan _offset = TimeSpan.Zero;

    public override DateTimeOffset GetUtcNow() => base.GetUtcNow() + _offset;

    public void Advance(TimeSpan amount) => _offset += amount;

    public void Reset() => _offset = TimeSpan.Zero;
}