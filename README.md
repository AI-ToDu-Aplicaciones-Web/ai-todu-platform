# AI-ToDu Platform

Identity, inventory and commercial services for Sprint 2, using C# 14, ASP.NET Core 10, Entity Framework Core and MySQL 8. The solution follows the backend folder conventions in the supplied Learning Center Platform guides and preserves the original Shared implementations.

## Open the projects

- Open `ai-todu-platform.sln` in Rider or Visual Studio.
- Restore dependencies with `dotnet restore`.
- Compile the solution with `dotnet build`.

## Start locally

Create an empty MySQL 8 database for development and grant the application user access to that database. These migrations initialize a MySQL schema; moving records from a former PostgreSQL installation requires a separate data import. Configure a random signing secret locally (at least 32 characters; never commit its value):

```powershell
dotnet user-secrets set "Authentication:SigningKey" "YOUR_RANDOM_SECRET_AT_LEAST_32_CHARACTERS" --project AiToDu.Platform
```

On the original development computer this secret has already been configured outside the repository. Configure the connection string and launch the API. Example for PowerShell; replace the connection details with your local database:

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:ConnectionStrings__DefaultConnection = 'Server=localhost;Port=3306;Database=ai_todu;User ID=ai_todu;Password=YOUR_LOCAL_PASSWORD'
$env:Database__MigrateOnStartup = 'true'
dotnet run --project AiToDu.Platform --no-launch-profile --urls http://localhost:5080
```

Open the frontend's `inventory.html#/sign-up` to create your organization and administrator, then sign in. The owner can register warehouse operators and sales clerks under **Users**. Data previously entered with the old demonstration organization stays in that organization; new accounts receive their own organization and inventory.

For hosting, configure secrets and token validation using environment variables:

```text
Authentication__SigningKey=<at least 32 characters, supplied as an environment secret>
Authentication__Issuer=ai-todu
Authentication__Audience=ai-todu-webapp
```

JWTs use HS256 and carry `sub` (user public UUID), `organization_id` (organization UUID), `role`, issuer, audience and expiration. Tokens last 60 minutes. Every protected request also checks that the account still exists and is active. `BUSINESS_OWNER` and `WAREHOUSE_OPERATOR` can manage inventory. `SALES_CLERK` can read it. Organization and actor are taken from the verified identity, never from request bodies. The host requires a signing key in every environment. The former demonstration authentication adapter was removed. Use HTTPS at the hosting edge.

Interactive OpenAPI documentation: `http://localhost:5080/scalar/v1`.
OpenAPI contract: `http://localhost:5080/openapi/v1.json`.
Database readiness: `http://localhost:5080/api/v1/health`.

For a controlled deployment, apply migrations separately rather than automatically:

```powershell
dotnet ef database update --project AiToDu.Platform --context WarehouseDbContext
dotnet ef database update --project AiToDu.Platform --context IamDbContext
```

The business calendar defaults to `America/Lima`. Override `Warehouse__TimeZone` if the deployment uses another business timezone. Audit timestamps are persisted in UTC.

## Structure

```text
AiToDu.Platform/
  Iam/
    Domain/{Model,Repositories}/
    Application/{CommandServices,QueryServices,Internal,Model,Exceptions}/
    Infrastructure/{Hashing,Tokens,Identity,Persistence}/
    Interfaces/Rest/{Resources,Transform}/
    Resources/
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
      Persistence/EntityFrameworkCore/{Configuration/Extensions,Repositories,Migrations}/
      Identity/
    Interfaces/
      Rest/{Resources,Transform}/
      Acl/
    Resources/
  Shared/
    Domain/{Model,Repositories}/
    Application/{Model,Internal/EventHandlers}/
    Infrastructure/{Persistence,Mediator,Pipeline,Interfaces}/
    Interfaces/Rest/ProblemDetails/
  Resources/{Shared,Errors}/
```

All 26 C# files under `Shared/` are reused from the Shared Bounded Context section of the supplied `learning-center-platform-guide.md`. Their implementations are preserved; only the `Acme.Center.Platform` namespace changes to `AiToDu.Platform`. The guide's two localization marker classes and four resource files are also included. [Source locations and normalized hashes](docs/shared-guide-origin.json) identify the reused C# files.

Warehouse adapts to those contracts: `InventoryItemId` wraps an integer, repositories extend the original `BaseRepository`, services use the original `Result` and `IUnitOfWork`, and events use the guide's Cortex contracts and interceptors. `WarehouseDbContext` extends the unchanged shared `AppDbContext` to supply Warehouse mappings and organization isolation. Identity adapters and domain-specific errors stay inside Warehouse.

`InventoryItem` is the consistency boundary. Lots, movements, physical counts, customer returns and processed operations are owned entities. They have no independent repositories or public mutators. Read the [architecture decisions](docs/warehouse-ddd.md) before changing this boundary.

Product IDs in API routes/resources are integers; lot and operation IDs remain UUIDs. This corrected initial migration targets a fresh database. An earlier local prototype used UUID product keys and must not share this initial migration history; preserve any existing data until a separate data migration is designed.

## API operations

IAM endpoints:

| Method | Route | Purpose |
| --- | --- | --- |
| POST | `/api/v1/authentication/sign-up` | Create a new organization owner. |
| POST | `/api/v1/authentication/sign-in` | Authenticate and issue a signed token. |
| GET | `/api/v1/authentication/me` | Retrieve the authenticated account. |
| GET / POST | `/api/v1/users` | Owner lists or registers users in their organization. |
| PATCH | `/api/v1/users/{id}/status` | Owner enables/disables a user in their organization. |

See [IAM behavior and manual walkthrough](docs/iam-sprint-2.md) for payloads and security boundaries.

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

Stock precision is three decimal places; prices have two. Negative inputs are rejected rather than clamped to zero. Only an Adjustment uses a signed quantity. Every accepted change saves its aggregate and owned records together. Expired and quarantined lots remain in physical stock but are excluded from available stock. Waste may remove quarantined or expired merchandise. A returned quantity classified as Quarantine receives a separate quarantined lot so that existing usable stock is not isolated accidentally.

## Integrating Sales and IAM

Sales opens a MySQL transaction on the shared operational connection and calls the Warehouse application facade to allocate available lots in expiry order. A failed line rolls back all writes. An accepted payment records a receipt; a rejected payment leaves the order pending. Retrying the same checkout reference and payload returns its original result. Returns use the original sale line and its lot.

| Method | Route | Purpose |
| --- | --- | --- |
| GET / POST | /api/v1/customers | Search or register customers |
| GET / POST | /api/v1/transactions | Filter history with from/to or submit a cart |
| GET | /api/v1/transactions/{id} | Sale detail |
| POST | /api/v1/transactions/{id}/payment or /cancel | Record payment or cancel a pending sale |
| GET | /api/v1/transactions/{id}/receipt | Confirmed receipt |
| POST | /api/v1/transactions/{id}/returns | Record a justified, classified return |
| PATCH | /api/v1/users/{id}/access | Set staff role and module permissions |
| GET / PUT | /api/v1/subscription | Read or select the organization plan |
| GET / POST | /api/v1/adjustment-reasons | Consult or register an adjustment reason |
| GET | /api/v1/inventory-items/by-sku/{sku} | Exact SKU lookup |
| GET | /api/v1/audit | Latest 200 authenticated successful write requests |
| GET | /api/v1/notifications | Stock and expiry alerts, including read state |
| PUT | /api/v1/notifications/{key}/read | Mark an alert read for the current user |

Starter includes 3 user accounts and 500 SKUs. Growth permits 10 user accounts and unlimited SKUs. Enterprise has unlimited accounts and SKUs. New organizations start with Starter. Registration and plan changes are serialized per organization. Stock alerts appear at or below the minimum; expiry alerts include lots with remaining stock expiring within 30 days and overdue lots.

Authentication/token issuance belongs to the implemented IAM context. The frontend reads its JWT from `aitodu_token` in session storage, validates the restored session through `/authentication/me`, and clears inventory snapshots when the account changes. There is no development or production authentication bypass.

## Build and event handling

```powershell
dotnet restore ai-todu-platform.sln
dotnet build ai-todu-platform.sln --no-restore
```

This delivery contains no test project or test scripts. Its CI compiles the solution.

The guide's `DispatchDomainEventsInterceptor` publishes events through Cortex after EF Core saves changes; `InventoryChangedEventHandler` currently logs the event. The original Shared implementation has no persistent outbox or durable delivery guarantee. Reliable delivery to other contexts remains an integration task.
