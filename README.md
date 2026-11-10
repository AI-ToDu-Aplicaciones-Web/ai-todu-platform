# AI-ToDu Warehouse Platform

Inventory services for Sprint 2, using C# 14, ASP.NET Core 10, Entity Framework Core and PostgreSQL. The solution follows the backend folder conventions in the supplied Learning Center Platform guides.

## Open the projects

- Open `ai-todu-platform.sln` in Rider or Visual Studio.
- Restore dependencies with `dotnet restore`.
- Run the rules and architecture checks with `dotnet test`.

## Start locally

Create an empty PostgreSQL database for development. Configure its connection string and launch the API. Example for PowerShell; replace the connection details with your local database:

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:ConnectionStrings__DefaultConnection = 'Host=localhost;Port=5432;Database=ai_todu;Username=postgres;Password=YOUR_LOCAL_PASSWORD'
$env:Database__MigrateOnStartup = 'true'
$env:Warehouse__AllowDevelopmentIdentity = 'true'
dotnet run --project AiToDu.Platform --no-launch-profile --urls http://localhost:5080
```

`AllowDevelopmentIdentity` explicitly enables a fixed local demonstration identity. It works only in the Development environment. Its organization is `11111111-1111-4111-8111-111111111111`; it has warehouse-operator permissions. The interface shows a banner whenever this identity is in use. This is a development integration aid, not the project's sign-in implementation.

For real IAM integration, leave that setting false and configure:

```text
Authentication__SigningKey=<at least 32 characters, supplied as an environment secret>
Authentication__Issuer=ai-todu
Authentication__Audience=ai-todu-webapp
```

JWTs use HS256 and must carry `sub` (actor UUID), `organization_id` (organization UUID), `role`, issuer, audience and expiration. `BUSINESS_OWNER` and `WAREHOUSE_OPERATOR` can manage inventory. `SALES_CLERK` can read it. Organization and actor are taken from the verified identity, never from request bodies. The production host rejects missing signing-key configuration and ignores demonstration identity even if its flag is true. Use HTTPS at the hosting edge.

Interactive OpenAPI documentation: `http://localhost:5080/scalar/v1`.
OpenAPI contract: `http://localhost:5080/openapi/v1.json`.
Database readiness: `http://localhost:5080/api/v1/health`.

For a controlled deployment, apply migrations separately rather than automatically:

```powershell
dotnet ef database update --project AiToDu.Platform
```

The business calendar defaults to `America/Lima`. Override `Warehouse__TimeZone` if the deployment uses another business timezone. Audit timestamps are persisted in UTC.

## Structure

```text
AiToDu.Platform/
  Warehouse/
    Domain/
      Model/{Aggregates,Entities,ValueObjects,Commands,Queries,Events,Errors}/
      Repositories/
    Application/
      CommandServices/
      QueryServices/
      Internal/{CommandServices,QueryServices}/
      OutboundServices/
      Acl/
    Infrastructure/
      Persistence/EntityFrameworkCore/{Configuration/Extensions,Repositories}/
      Identity/
    Interfaces/
      Rest/{Resources,Transform}/
      Acl/
    Resources/
  Shared/
    Domain/{Model,Repositories}/
    Application/{Model,Exceptions}/
    Infrastructure/{Persistence,Security,Interfaces}/
```

`InventoryItem` is the consistency boundary. Lots, movements, physical counts, customer returns and processed operations are owned entities. They have no independent repositories or public mutators. Read the [architecture decisions](docs/warehouse-ddd.md) before changing this boundary.

## API operations

| Method | Route | Purpose |
| --- | --- | --- |
| GET | `/api/v1/warehouse/session` | Verified identity and inventory permissions. |
| GET | `/api/v1/inventory-items?search=RICE-01&page=1&pageSize=20` | Exact SKU or partial name, scoped to the current organization. |
| POST | `/api/v1/inventory-items` | Register a product with zero initial stock. |
| GET | `/api/v1/inventory-items/{id}` | Product, lots, history, counts and classified returns. |
| POST | `/api/v1/inventory-items/{id}/lots` | Receive merchandise, specifying lot/location/expiry. |
| POST | `/api/v1/inventory-items/{id}/movements` | Issue, signed adjustment or waste. |
| POST | `/api/v1/inventory-items/{id}/counts` | Apply a physical count against a stated baseline. |
| POST | `/api/v1/inventory-items/{id}/returns` | Classify a customer return as Restock, Quarantine or Rejected. |
| PATCH | `/api/v1/inventory-items/{id}/lots/{lotId}/quarantine` | Isolate or release a lot, with a reason. |
| PATCH | `/api/v1/inventory-items/{id}/lots/{lotId}/location` | Relocate a lot, with a reason. |

Registration body:

```json
{"sku":"RICE-01","name":"Rice","category":"Food","minimumStock":5,"unitPrice":10}
```

Receipt body, after using the version returned by registration:

```json
{"operationId":"33333333-3333-4333-8333-333333333333","expectedVersion":1,"lotCode":"LOT-01","location":"A-1","expiresOn":"2099-12-31","quantity":20}
```

All stock-changing commands require a new `operationId` and the latest `expectedVersion`. Reuse the *same operation ID and payload* when retrying an uncertain request. An identical replay has no additional effect, even when its original expected version is old. Reusing an ID with changed data is rejected. A stale write returns 409; reload the product and review the intended operation before trying again. A creation retry is protected against duplicate SKU but is not treated as an idempotent success.

Stock precision is three decimal places; prices have two. Negative inputs are rejected rather than clamped to zero. Only an Adjustment uses a signed quantity. Every accepted change commits its aggregate and integration-event record together. Expired and quarantined lots remain in physical stock but are excluded from available stock. Waste may remove quarantined or expired merchandise. A returned quantity classified as Quarantine receives a separate quarantined lot so that existing usable stock is not isolated accidentally.

## Integrating Sales and IAM

`IWarehouseContextFacade` exposes primitive identifiers and availability/issue operations; another context must not reference Warehouse entities or repositories. The issue operation has its own Sales policy: an authenticated sales clerk or business owner may issue through that internal use case without receiving general warehouse-adjustment permissions. It has no general HTTP write endpoint. The issue operation is idempotent by its operation reference. It commits a **Warehouse transaction**; it does not make a Sales transaction atomic across contexts. Sales should coordinate its own state and reliable integration events/compensation instead of updating stock from the browser before saving a sale. Do not deduct the same sale again at dispatch.

Authentication/token issuance belongs to IAM and is not implemented by this inventory module. The frontend reads a JWT from `aitodu_token` in session/local storage; agree the final storage and claims contract with IAM. There is no production bypass.

## Verification

`dotnet test` covers inventory invariants, retry semantics, stale versions, count baselines, expiry/business dates, quarantine, return classification and encapsulation.

For a running test API and **disposable** database, with a test signing key matching the API:

```powershell
$env:WAREHOUSE_TEST_SIGNING_KEY = '<test-only signing key>'
python tests/warehouse_api_smoke.py
```

The integration script creates isolated test organizations/products, checks permissions and tenant separation, persists and reloads stock, tests concurrent deductions and verifies OpenAPI. It does not delete existing business records. Do not run it against production.

Domain events are committed to `shared.outbox_messages`. A background delivery worker is a future integration responsibility; this module does not claim that events have already been delivered to other contexts.
