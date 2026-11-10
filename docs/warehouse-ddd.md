# Warehouse DDD and reuse of the backend guide

## Shared comes from the supplied guide

The authoritative backend reference is `learning-center-platform-guide.md`. All 26 C# files in its Shared Bounded Context section are included under `Shared/`, with only `Acme.Center.Platform` changed to `AiToDu.Platform`. No Warehouse-specific class is added to Shared. The resource marker classes and English/Spanish localization resources accompany them. `shared-guide-origin.json` records the original C# block locations and SHA-256 hashes after namespace substitution and LF normalization.

This includes the aggregate and identity contracts, `Error`, `Result`, generic repository and Unit of Work, EF context and converters, auditing and event interceptors, Cortex interfaces and command behaviors, REST ProblemDetails factory, exception handler, routing conventions and OpenAPI transformer. The host registers these components; the direct command/query services use the guide's service contracts and Unit of Work.

Warehouse extends the unchanged Shared `AppDbContext` through `WarehouseDbContext`. The derived context supplies only Warehouse mappings, tenant filtering and translation of persistence conflicts. Migrations reside in Warehouse infrastructure. Product identity uses `IEntityId<int>` to match the original generic repository contract; lot identities use `IEntityId<Guid>`. Actor and organization identities belong to Warehouse rather than modifying Shared.

## Aggregate boundary

`InventoryItem` owns `StockLot`, `InventoryMovement`, `StockCount`, `InventoryReturn` and `InventoryOperation`. Every stock mutation goes through the aggregate. Child entities have no independent repository or public balance mutation API. Collections are exposed read-only. SKU, quantity, location, lot code and reason validate their own values.

Physical stock is the sum of the lots. Available stock excludes quarantined or expired lots using the business date. Physical counts require the expected recorded balance. Rejected returns add no stock; quarantined returns create a separate quarantined lot. Sales references are external values, without navigation to another context's aggregate.

## Dependencies and folders

Warehouse follows `Domain/Model`, `Domain/Repositories`, `Application/{CommandServices,QueryServices,Internal,OutboundServices,Acl}`, `Infrastructure` and `Interfaces/{Rest,Acl}`. The domain uses the BCL and Shared Domain contracts. The original Shared `IEvent` inherits Cortex's notification contract, exactly as in the guide. EF Core mappings and identity adapters stay in Infrastructure; REST resources and assemblers stay in Interfaces.

Application orchestrates the aggregate, repository, identity/clock ports and the original `IUnitOfWork`. Controllers assemble resources and delegate. `Application/Acl` implements the published `Interfaces/Acl` contract. No inventory balance is computed or saved by a controller.

## Persistence, concurrency and events

EF Core saves the aggregate and its owned state in a Warehouse transaction. A unique organization/SKU index protects duplicate registration. An optimistic-concurrency version rejects stale writes. An operation ID and payload signature prevent a retry from applying a stock change twice. Database errors are translated outside Shared into Warehouse application conflicts.

The exact guide `AggregateRoot` stores deferred event factories. After saving, the original `DispatchDomainEventsInterceptor` evaluates the events, clears them from the aggregates and publishes them through Cortex. The Warehouse handler currently logs inventory events. There is no persistent outbox in this implementation and no guarantee of durable delivery to Sales or Logistics. A later integration must address that explicitly without claiming the guide's interceptor provides it.

Audit history currently loads with the aggregate. Larger histories will need read models and an explicit retention strategy that preserves retry guarantees.

## Integration boundaries

The implemented IAM context owns login, token issuance and organization membership. Warehouse receives the verified actor and organization through its existing application port; request bodies cannot choose them. Authentication infrastructure lives in IAM and rejects disabled accounts even if their token has not expired. The old development identity adapter has been removed.

Sales uses the ACL availability/issue contract rather than Warehouse repositories. The stock issue and Warehouse journal are atomic within Warehouse; completing a sale across contexts still needs coordination. Dispatch must not deduct the same sale again.

The inventory operations are preserved. The existing landing opens `inventory.html`; its router now includes IAM screens and requires a real session for inventory. Sales integration and cloud deployment remain separate work. The corrected Warehouse initial migration uses integer product IDs and is intended for a fresh database, preserving the earlier local prototype database. IAM adds its own schema and migration history to the corrected database, without changing Warehouse tables or Shared.

## Delivery validation

The solution is compiled with `dotnet build`. Test projects, scripts and CI test steps were removed at the user's request. The source provenance comparison verifies literal reuse of Shared after the namespace change; it is not a behavioral test suite.
