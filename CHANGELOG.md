# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-11-15

### Added
- Production Release `v1.0.0` of **AI-ToDu RESTful Platform (`AiToDu.Platform`)** built with C#, ASP.NET Core (.NET 10), Entity Framework Core, Domain-Driven Design (DDD), and CQRS.
- `Shared` Kernel: `AggregateRoot`, `IAggregateRoot`, `IEvent` (`Cortex.Mediator`), `Result<T>` and `Error` catalog with `RFC 9457 ProblemDetailsFactory`, `UnitOfWork`, and `DispatchDomainEventsInterceptor`.
- `Iam` Bounded Context: JWT authentication, BCrypt password hashing, organization user lifecycle, and role/permission access control.
- `Warehouse` Bounded Context: `InventoryItem` aggregate root, strongly-typed IDs, lot quarantine control, stock classification, and Anti-Corruption Layer (ACL).
- `Sales`, `Subscriptions`, and `Activity` Bounded Contexts with bilingual localization (`en-US` and `es-419`) and OpenAPI / Scalar interactive documentation (`/scalar/v1`).
 