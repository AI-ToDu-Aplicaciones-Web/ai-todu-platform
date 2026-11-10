namespace AiToDu.Platform.Warehouse.Application.OutboundServices;

/// <summary>Business clock; preserves the organization's configured calendar date.</summary>
public interface IWarehouseClock
{
    DateTimeOffset GetCurrentTime();
}
