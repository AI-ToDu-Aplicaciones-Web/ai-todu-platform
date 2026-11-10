using AiToDu.Platform.Shared.Domain.Model.Entities;
using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
using AiToDu.Platform.Warehouse.Domain.Model.Errors;
using AiToDu.Platform.Shared.Domain.Model.Aggregates;
using AiToDu.Platform.Warehouse.Domain.Model.Entities;
using AiToDu.Platform.Warehouse.Domain.Model.Events;

namespace AiToDu.Platform.Warehouse.Domain.Model.Aggregates;

/// <summary>Consistency boundary for a product, its lots, counts, returns and movement ledger.</summary>
public sealed class InventoryItem : AggregateRoot, IAuditableEntity
{
    private readonly List<StockLot> _lots = [];
    private readonly List<InventoryMovement> _movements = [];
    private readonly List<StockCount> _counts = [];
    private readonly List<InventoryReturn> _returns = [];
    private readonly List<InventoryOperation> _operations = [];
    public InventoryItemId Id { get; private set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public OrganizationId OrganizationId { get; private set; }
    public Sku Sku { get; private set; } = null!;
    public string Name { get; private set; } = "";
    public string Category { get; private set; } = "";
    public StockQuantity MinimumStock { get; private set; }
    public decimal UnitPrice { get; private set; }
    public long Version { get; private set; }
    public IReadOnlyCollection<StockLot> Lots => _lots.AsReadOnly();
    public IReadOnlyCollection<InventoryMovement> Movements => _movements.AsReadOnly();
    public IReadOnlyCollection<StockCount> Counts => _counts.AsReadOnly();
    public IReadOnlyCollection<InventoryReturn> Returns => _returns.AsReadOnly();
    public IReadOnlyCollection<InventoryOperation> Operations => _operations.AsReadOnly();
    public decimal TotalQuantity => _lots.Sum(x => x.Quantity.Value);
    public decimal AvailableQuantityOn(DateOnly date) => _lots.Where(x => x.IsAvailableOn(date)).Sum(x => x.Quantity.Value);
    private InventoryItem() { }
    public InventoryItem(InventoryItemId id, OrganizationId organizationId, Sku sku, string name,
        string category, StockQuantity minimumStock, decimal unitPrice, ActorId actorId, DateTimeOffset now)
    {
        if (id.Value < 0 || organizationId.Value == Guid.Empty || actorId.Value == Guid.Empty) throw new WarehouseDomainException(WarehouseErrorCode.InvalidIdentifier);
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 160) throw new WarehouseDomainException(WarehouseErrorCode.InvalidName);
        if (string.IsNullOrWhiteSpace(category) || category.Trim().Length > 100) throw new WarehouseDomainException(WarehouseErrorCode.InvalidCategory);
        if (unitPrice < 0 || unitPrice > 999999999999m || decimal.Round(unitPrice, 2) != unitPrice) throw new WarehouseDomainException(WarehouseErrorCode.InvalidPrice);
        Id = id; OrganizationId = organizationId; Sku = sku; Name = name.Trim(); Category = category.Trim();
        MinimumStock = minimumStock; UnitPrice = unitPrice; Version = 1;
        AddDomainEvent(() => new InventoryChangedEvent(Guid.NewGuid(), OrganizationId, Id, Guid.NewGuid(), "InventoryItemRegistered", actorId, now.ToUniversalTime()));
    }
    public void ReceiveLot(Guid operationId, long expectedVersion, LotCode code, StorageLocation location,
        DateOnly? expiresOn, StockQuantity quantity, ActorId actorId, DateTimeOffset now)
    {
        Execute(operationId, expectedVersion, Signature("receipt", code.Value, location.Value, expiresOn, quantity.Value, actorId), actorId, now, () =>
        {
            StockQuantity.Positive(quantity.Value);
            if (expiresOn < DateOnly.FromDateTime(now.DateTime)) throw new WarehouseDomainException(WarehouseErrorCode.ExpiredReceipt);
            var lot = _lots.SingleOrDefault(x => x.Code == code && x.Location == location);
            if (lot is not null && lot.ExpiresOn != expiresOn) throw new WarehouseDomainException(WarehouseErrorCode.LotExpiryConflict, true);
            var next = new StockQuantity((lot?.Quantity.Value ?? 0) + quantity.Value);
            if (lot is null) { lot = new StockLot(StockLotId.New(), code, location, expiresOn, false); _lots.Add(lot); }
            Change(lot, next.Value, MovementType.Receipt, "Merchandise received", operationId, actorId, now);
        });
    }
    public void RecordMovement(Guid operationId, long expectedVersion, StockLotId lotId,
        MovementType type, decimal quantity, MovementReason reason, ActorId actorId, DateTimeOffset now)
    {
        Execute(operationId, expectedVersion, Signature("movement", lotId, type, quantity, reason.Value, actorId), actorId, now, () =>
        {
            var lot = GetLot(lotId);
            if (type is not (MovementType.Issue or MovementType.Waste or MovementType.Adjustment)) throw new WarehouseDomainException(WarehouseErrorCode.InvalidMovementType);
            if (type == MovementType.Adjustment)
            { if (quantity == 0) throw new WarehouseDomainException(WarehouseErrorCode.ZeroAdjustment); if (quantity == decimal.MinValue) throw new WarehouseDomainException(WarehouseErrorCode.InvalidQuantity); new StockQuantity(Math.Abs(quantity)); }
            else StockQuantity.Positive(quantity);
            if (type == MovementType.Issue && !lot.IsAvailableOn(DateOnly.FromDateTime(now.DateTime))) throw new WarehouseDomainException(WarehouseErrorCode.LotUnavailable, true);
            var next = lot.Quantity.Value + (type == MovementType.Adjustment ? quantity : -quantity);
            if (next < 0) throw new WarehouseDomainException(WarehouseErrorCode.InsufficientStock, true);
            new StockQuantity(next);
            Change(lot, next, type, reason.Value, operationId, actorId, now);
        });
    }
    public void RegisterCount(Guid operationId, long expectedVersion, StockLotId lotId,
        StockQuantity expected, StockQuantity counted, MovementReason reason, ActorId actorId, DateTimeOffset now)
    {
        Execute(operationId, expectedVersion, Signature("count", lotId, expected.Value, counted.Value, reason.Value, actorId), actorId, now, () =>
        {
            var lot = GetLot(lotId);
            if (lot.Quantity != expected) throw new WarehouseDomainException(WarehouseErrorCode.CountOutdated, true);
            _counts.Add(new StockCount(operationId, lotId, expected.Value, counted.Value, reason.Value, actorId, now));
            if (counted != expected) Change(lot, counted.Value, MovementType.CountAdjustment, reason.Value, operationId, actorId, now);
        });
    }
    public void ClassifyReturn(Guid operationId, long expectedVersion, StockLotId sourceLotId,
        StockQuantity quantity, ReturnDisposition disposition, string reference, MovementReason reason,
        ActorId actorId, DateTimeOffset now)
    {
        Execute(operationId, expectedVersion, Signature("return", sourceLotId, quantity.Value, disposition, reference, reason.Value, actorId), actorId, now, () =>
        {
            StockQuantity.Positive(quantity.Value);
            if (!Enum.IsDefined(disposition)) throw new WarehouseDomainException(WarehouseErrorCode.InvalidDisposition);
            if (string.IsNullOrWhiteSpace(reference) || reference.Trim().Length > 120) throw new WarehouseDomainException(WarehouseErrorCode.ReturnReferenceRequired);
            var source = GetLot(sourceLotId);
            StockLot? destination = null;
            if (disposition == ReturnDisposition.Restock)
            {
                if (!source.IsAvailableOn(DateOnly.FromDateTime(now.DateTime))) throw new WarehouseDomainException(WarehouseErrorCode.LotUnavailable, true);
                new StockQuantity(source.Quantity.Value + quantity.Value); destination = source;
            }
            else if (disposition == ReturnDisposition.Quarantine)
                destination = new StockLot(StockLotId.New(), new LotCode("RETURN-" + operationId.ToString("N")), source.Location, source.ExpiresOn, true);
            if (destination is not null)
            {
                if (destination != source) _lots.Add(destination);
                Change(destination, destination.Quantity.Value + quantity.Value, MovementType.CustomerReturn, reason.Value, operationId, actorId, now);
            }
            _returns.Add(new InventoryReturn(operationId, sourceLotId, destination?.Id, quantity.Value, disposition, reference.Trim(), reason.Value, actorId, now));
        });
    }
    public void SetQuarantine(Guid operationId, long expectedVersion, StockLotId lotId, bool quarantine,
        MovementReason reason, ActorId actorId, DateTimeOffset now) =>
        Execute(operationId, expectedVersion, Signature("quarantine", lotId, quarantine, reason.Value, actorId), actorId, now, () => GetLot(lotId).SetQuarantine(quarantine));
    public void RelocateLot(Guid operationId, long expectedVersion, StockLotId lotId, StorageLocation location,
        MovementReason reason, ActorId actorId, DateTimeOffset now) =>
        Execute(operationId, expectedVersion, Signature("relocate", lotId, location.Value, reason.Value, actorId), actorId, now, () =>
        {
            var lot = GetLot(lotId);
            if (_lots.Any(x => x.Id != lotId && x.Code == lot.Code && x.Location == location)) throw new WarehouseDomainException(WarehouseErrorCode.LotLocationConflict, true);
            lot.Relocate(location);
        });
    private StockLot GetLot(StockLotId id) => _lots.SingleOrDefault(x => x.Id == id) ?? throw new WarehouseDomainException(WarehouseErrorCode.LotNotFound);
    private void Change(StockLot lot, decimal next, MovementType type, string reason, Guid operationId, ActorId actorId, DateTimeOffset now)
    {
        var movement = new InventoryMovement(operationId, lot.Id, type, lot.Quantity.Value, next, reason, actorId, now);
        lot.ChangeQuantity(next); _movements.Add(movement);
    }
    private void Execute(Guid operationId, long expectedVersion, string signature, ActorId actorId, DateTimeOffset now, Action action)
    {
        if (operationId == Guid.Empty) throw new WarehouseDomainException(WarehouseErrorCode.OperationIdRequired);
        if (actorId.Value == Guid.Empty) throw new WarehouseDomainException(WarehouseErrorCode.InvalidIdentifier);
        var previous = _operations.SingleOrDefault(x => x.Id == operationId);
        if (previous is not null)
        { if (previous.Signature != signature) throw new WarehouseDomainException(WarehouseErrorCode.OperationReused, true); return; }
        if (Version != expectedVersion) throw new WarehouseDomainException(WarehouseErrorCode.VersionConflict, true);
        action(); Version++;
        _operations.Add(new InventoryOperation(operationId, signature));
        var separator = signature.IndexOf(':');
        var changeType = signature.Substring(separator + 1, int.Parse(signature[..separator], System.Globalization.CultureInfo.InvariantCulture));
        AddDomainEvent(() => new InventoryChangedEvent(Guid.NewGuid(), OrganizationId, Id, operationId, changeType, actorId, now.ToUniversalTime()));
    }
    private static string Signature(params object?[] values) => string.Concat(values.Select(value =>
    {
        var text = Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) ?? "";
        return text.Length + ":" + text;
    }));
}
