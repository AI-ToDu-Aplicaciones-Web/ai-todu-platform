using AiToDu.Platform.Warehouse.Application.OutboundServices;

namespace AiToDu.Platform.Warehouse.Infrastructure.Identity;

public sealed class WarehouseClock(TimeProvider time, IConfiguration configuration) : IWarehouseClock
{
    private readonly TimeZoneInfo _zone = TimeZoneInfo.FindSystemTimeZoneById(configuration["Warehouse:TimeZone"] ?? "America/Lima");
    public DateTimeOffset GetCurrentTime() => TimeZoneInfo.ConvertTime(time.GetUtcNow(), _zone);
}
