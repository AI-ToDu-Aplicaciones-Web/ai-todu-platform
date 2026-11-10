# Warehouse DDD decisions

## Reference alignment

The supplied `learning-center-platform-guide.md` and study guide provide the backend conventions: bounded context first; `Domain/Model` for aggregates, value objects, commands, queries, events and errors; repository contracts in Domain; command/query contracts and implementations in Application; EF Core only in Infrastructure; REST resources and assemblers in Interfaces. The frontend follows `learning-center-guide.md`: `domain/model`, `application`, `infrastructure`, `presentation/views`, context routes and assemblers.

## Aggregate boundary and rules

- `InventoryItem` owns `StockLot`, `InventoryMovement`, `StockCount`, `InventoryReturn` and `InventoryOperation`. Every stock mutation goes through its methods.
- Aggregate and foreign identity references use value objects. SKU, lot code, location, reason and quantity validate their own values. Collections are exposed read-only and child mutations are internal.
- Physical stock is the sum of lot quantities. Available stock excludes quarantined/expired lots on the business calendar date. There is no second editable stock total that can diverge from its lots.
- Counts apply only to the expected recorded quantity. A mismatch requires recount/review rather than silently overwriting a newer balance.
- Rejected returns do not increase stock. Quarantined returns have a separate destination lot. The returned sale reference is an external reference, not a foreign navigation to a Sales aggregate.
- Reasons and verified actors are recorded for quantity changes. Operation signatures additionally retain the details of quarantine/location commands; domain-event records retain actor, operation and time.

## Dependencies

Domain uses only the BCL and Shared Domain. It has no EF, ASP.NET, JSON or Vue dependencies. Application services orchestrate repositories, identity and clock ports and the Unit of Work. Controllers assemble resource/command data and HTTP results; they do not change balances. Infrastructure implements persistence/identity/clock ports. `Application/Acl` implements the published `Interfaces/Acl` contract following the supplied guide.

The host wires dependencies. The guide uses a mediator adapter for event delivery; here domain events have a framework-free contract and are persisted in an outbox with the aggregate. This keeps domain invariants independent of the mediator and prevents claiming reliable cross-context delivery before a dispatcher exists.

## Transaction and concurrency

EF Core maps owned entities in Warehouse tables. A unique organization/SKU index closes the race left by an application-only duplicate check. The aggregate version is an optimistic-concurrency token. One SaveChanges transaction includes the root, owned state and outbox; a losing concurrent command rolls back its movements. Provider exceptions are translated into application-level conflicts.

`operationId` plus a canonical signature prevents an uncertain request from applying twice. A matching replay does not create movements, increment version or add events. A changed payload with the same operation ID is a conflict.

The aggregate currently loads its audit collections with its state. This favors a clear, testable Sprint 2 boundary. At larger history volumes, introduce bounded read models and an archival/operation-retention policy while preserving the same mutation invariants; do not discard idempotency records arbitrarily.

## Integration boundaries

IAM owns authentication and tenant membership. Warehouse receives a verified organization/actor through an application port. It never trusts organization identifiers from request JSON. Sales/Logistics use the ACL and published events; they do not reach into Warehouse persistence. A stock issue and its Warehouse journal are atomic, while Sales/Inventory cross-context workflow needs its own coordination.

The older unused frontend `inventory/` model/service were removed after confirming there were no callers. `warehouse/` is the inventory implementation. The existing landing is preserved; its warehouse CTA now opens the new inventory entry. Other contexts remain outside this change.
