# Pharmacy Management System - Architecture

## 1. Overview

A production-oriented, multi-branch Pharmacy Management System built with:

- **.NET 10 (LTS)** / ASP.NET Core
- **ASP.NET Core Web API** (REST, JWT)
- **Windows Forms (.NET 10)** desktop client using `HttpClient` with bearer tokens
- **Entity Framework Core 10** with **SQL Server 2022**
- **ASP.NET Core Identity** (password hashing, lockout, roles)
- **FluentValidation**, **Serilog**, **Swashbuckle/OpenAPI**
- **xUnit** unit and SQL Server integration tests

The Windows Forms client and the API are separate processes over the same Application/Infrastructure layers and the same database.
The desktop client stores JWT access/refresh tokens locally and sends bearer tokens on every request; no web cookies are used.
All business rules live in the Application layer and are shared by both hosts.

## 2. Solution Structure

```
PharmacyManagement/
├── PharmacyManagement.slnx
├── docs/
│   ├── 01-ARCHITECTURE.md
│   ├── 02-DATABASE.md
│   ├── 03-API.md
│   └── 04-ROADMAP.md
├── database/                       # generated idempotent SQL script from EF migrations
├── src/
│   ├── PharmacyManagement.Domain/
│   │   ├── Common/                 # BaseEntity, AuditableEntity, ISoftDeletable
│   │   ├── Entities/               # 35+ domain entities
│   │   ├── Enums/
│   │   ├── Interfaces/
│   │   └── Exceptions/
│   ├── PharmacyManagement.Application/
│   │   ├── Common/                 # PagedResult, QueryParameters, Permissions, interfaces
│   │   ├── DTOs/                   # per-module request/response DTOs
│   │   ├── Interfaces/             # service + infrastructure abstractions
│   │   ├── Services/               # business logic (sales, inventory, purchases, ...)
│   │   ├── Validators/             # FluentValidation validators
│   │   ├── Mappings/               # manual mapping extensions
│   │   └── DependencyInjection.cs
│   ├── PharmacyManagement.Infrastructure/
│   │   ├── Persistence/            # PharmacyDbContext, configurations, views, seed
│   │   ├── Repositories/           # generic repository
│   │   ├── Services/               # JWT, Identity auth, file storage, exports, dashboard queries
│   │   ├── Migrations/
│   │   └── DependencyInjection.cs
│   ├── PharmacyManagement.API/
│   │   ├── Controllers/
│   │   ├── Middleware/             # exception handling, audit context
│   │   ├── Filters/                # FluentValidation action filter
│   │   ├── Extensions/
│   │   ├── Program.cs
│   │   └── appsettings.json
│   └── PharmacyManagement.WinForms/
│       ├── Configuration/          # API base URL and session settings
│       ├── Controls/               # reusable grids and module screens
│       ├── Forms/                  # login, main shell, object editor
│       ├── Services/               # ApiClient, TokenStore, ObjectMapper
│       ├── Program.cs
│       └── appsettings.json
└── tests/
    └── PharmacyManagement.Tests/
        ├── Unit/                   # FEFO, pricing, business rules
        └── Integration/            # WebApplicationFactory + SQL Server test DB
```

## 3. Layering Rules

| Layer | Depends on | Responsibility |
|---|---|---|
| Domain | - | Entities, enums, domain exceptions, interfaces with no infrastructure knowledge |
| Application | Domain | Business rules, DTOs, validators, service contracts, EF Core abstractions |
| Infrastructure | Application, Domain | EF Core DbContext/configurations/migrations, Identity, JWT, exports, file storage, repositories |
| API | Application, Infrastructure | REST endpoints, JWT auth, Swagger, middleware, DI composition |
| WinForms | Application, Domain | Desktop UI, JWT `HttpClient`, role-aware navigation |
| Tests | API, Application, Infrastructure | Unit + integration tests against SQL Server |

- Controllers are thin: validate/authorize, call an application service, return a result.
- No EF queries in controllers.
- DTOs are never database entities; entities never leave the service layer through the API.
- Cross-cutting concerns: centralized exception middleware, FluentValidation filter, Serilog, audit logging.

## 4. Authentication & Authorization

### API
- `POST /api/auth/login` validates credentials through Identity (`UserManager`, lockout enabled).
- Access token: JWT, 15 minutes, signed HMAC-SHA256.
- Refresh token: 64 random bytes, SHA-256 hash persisted, 7 days, rotated on use, revoked on logout.
- Claims: `sub`, `email`, `name`, `role`, `permission`, `branch_id`, `jti`.
- JWT bearer authentication; HTTPS enforced outside Development.

### Windows Forms
- Login form posts credentials to `POST /api/auth/login`.
- Access and refresh tokens are held by `TokenStore`; `ApiClient` attaches the bearer token to every request.
- On 401, the client attempts one refresh via `POST /api/auth/refresh` before returning to login.
- MainForm navigation is filtered by the permissions returned in the login/user profile response.

### RBAC + Permissions
- Roles: `SuperAdmin`, `PharmacyAdmin`, `Pharmacist`, `Cashier`, `InventoryManager`.
- Permission catalog stored in `Permissions`, mapped to roles through `RolePermissions`.
- Policies are generated from permission codes (e.g. `Medicines.Create`).
- Backend enforcement on every endpoint via `[Authorize(Policy = ...)]`; the desktop UI hides unauthorized navigation but is never the security boundary.
- SuperAdmin implicitly receives every permission.

## 5. Business Rule Enforcement

| Rule | Enforcement |
|---|---|
| Never negative inventory | Quantity checks + rowversion concurrency + serializable transactions |
| Never sell expired medicine | Batch eligibility filter `ExpiryDate > today` |
| FEFO | `FefoAllocator` orders eligible batches by expiry date, then received date |
| Prescription-only medicines | Verified prescription required and medicine must be on the prescription |
| Purchase receiving increases stock | Transaction: goods receipt + batch upsert + inventory movement |
| Sales decrease correct batch | Transaction: sale + details + batch deductions + movements + payments |
| Returns require approval/inspection | `Pending -> Approved/Rejected`; restock only when approved, resalable and unexpired |
| Financial records are permanent | No hard delete of sales/purchases/payments/returns; cancellation/status changes only |
| Audit trail | Sensitive operations write `AuditLogs` with user, entity, before/after values, IP |
| Actual batch cost for profit | `SaleDetail.UnitCost` captured from the consumed batch at sale time |
| Multi-branch stock | Every batch, movement, sale, purchase and transfer is branch-scoped |
| Document numbers | Atomic `DocumentSequence` increment inside the same transaction |

## 6. Transaction & Concurrency Strategy

- Sales, purchases receiving, returns approval, stock adjustments and transfers run inside a database transaction.
- `MedicineBatch.RowVersion` (`rowversion`) is an optimistic concurrency token.
- Sales use `IsolationLevel.Serializable` for batch selection/deduction to prevent overselling.
- `DbUpdateConcurrencyException` is translated into a user-facing conflict ("stock changed, retry").
- `DocumentSequence` numbers are allocated with a single atomic `UPDATE ... OUTPUT` statement.

## 7. Reporting & Exports

- SQL views: `vw_MedicineStock`, `vw_ExpiringBatches`, `vw_LowStockMedicines`.
- Stored procedures: `usp_NextDocumentNumber`, `usp_GetDashboardSummary` (optional/where beneficial).
- Report queries are computed from transactional tables; no hardcoded statistics.
- Excel export: ClosedXML. PDF export: QuestPDF. Printing is done from the desktop client via the generated PDF.
- Date filters are inclusive, expressed as UTC day boundaries.

## 8. Security Controls

- Password hashing and lockout via ASP.NET Core Identity (no plaintext passwords).
- JWT signing key and seed password come from configuration/user-secrets/environment variables.
- Role + permission authorization on backend.
- FluentValidation on every write DTO.
- Parameterized EF Core queries (no string-concatenated SQL).
- File uploads: size/extension allow-list, stored in a configured folder outside the app directory, served through authorized endpoints.
- Centralized exception handling (no stack traces in production responses).
- Serilog structured logs + persisted audit log table.
- HTTPS redirection and HSTS in production; CORS is configured for optional browser clients, while the WinForms client uses bearer auth directly.
- Account lockout after 5 failed attempts (15 minutes).
- Refresh token rotation and revocation.

## 9. UI Design System

- Enterprise desktop light theme: white/soft-gray surfaces, deep teal primary color, neutral typography.
- Fixed sidebar navigation with role-aware menu, top bar with branch selector and user menu.
- Dashboard cards with real KPIs, charts and alert panels rendered with native WinForms controls.
- Consistent `DataGridView` tables with search, filters, pagination, `PropertyGrid` editors, modal dialogs and status badges.
- Resizable DPI-aware shell with horizontally scrollable tables.
- No excessive gradients/animations; accessible contrast and focus states.

## 10. Configuration

- `appsettings.json` contains non-secret defaults and connection string placeholders.
- Secrets via environment variables / user-secrets:
  - `ConnectionStrings__DefaultConnection`
  - `Jwt__Secret`
  - `Seed__AdminPassword`
- The API and desktop client share the same database and application services.
- Startup applies EF migrations and seeds reference data in Development (configurable).
