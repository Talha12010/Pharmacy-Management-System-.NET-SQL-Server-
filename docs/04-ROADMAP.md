# Development Roadmap & Delivery Checklist

## Phase 1 - Planning (complete before implementation)

- [x] Requirements analysis
- [x] Layered architecture (`docs/01-ARCHITECTURE.md`)
- [x] Normalized schema, constraints, indexes (`docs/02-DATABASE.md`)
- [x] ERD (Mermaid)
- [x] API endpoint plan (`docs/03-API.md`)
- [x] Roles and permission catalog
- [x] Roadmap

## Phase 2 - Database & Backend Foundation

- [x] .NET 10 solution and projects
- [x] EF Core + SQL Server configuration
- [x] Domain entities, enums, configurations
- [x] Identity, JWT, refresh tokens, lockout
- [x] Permission policies and claims
- [x] Central exception middleware, validation filter, Serilog
- [x] Audit logging and current-user service
- [x] Initial migration + reporting views/sequences
- [x] Seed permissions/roles/admin/reference data

## Phase 3 - Core Modules

- [x] Medicine catalog (categories, manufacturers, units, medicines)
- [x] Suppliers and customers (statements, balances, payments)
- [x] Batch inventory, stock in/out/adjustments, movements
- [x] Stock transfers between branches
- [x] Low stock / expiring / expired alerts

## Phase 4 - Business Modules

- [x] Purchase orders, approval workflow, goods receiving (partial)
- [x] POS sale with FEFO, payments, invoices, cancellation
- [x] Prescription registration, verification, dispensing enforcement
- [x] Sale and purchase returns with inspection/restock/refund workflow
- [x] Expenses and categories
- [x] Employees and user accounts
- [x] Customer/supplier payments and statements

## Phase 5 - Windows Forms Client

- [x] Enterprise desktop shell: sidebar, topbar, DPI-aware resizable layout
- [x] Login form with JWT token storage and automatic refresh
- [x] Dashboard KPIs, charts, alerts, recent transactions
- [x] All management screens (CRUD, filters, pagination)
- [x] POS screen with barcode/search, cart, split payments, receipt
- [x] Purchase and receiving screens
- [x] Prescription, returns, expenses, employees, users, settings screens
- [x] Reports hub with filters, print, Excel/PDF export

## Phase 6 - Testing & Optimization

- [x] Unit tests: FEFO, pricing, validators, business rules
- [x] Integration tests: health, login, protected dashboard, medicine lookup, reports
- [x] SQL Server database with migrations and idempotent seed
- [x] Index review for common filters/lookups
- [x] Validation and error handling checks
- [x] Concurrency and stock-integrity checks enforced in sale/inventory services

## Phase 7 - Finalization

- [x] Idempotent seed data and secure initial admin
- [x] Generated SQL scripts (`database/`)
- [x] Environment variables/secrets documented
- [x] Installation and deployment documentation (`README.md`)
- [x] Final build and test run

## Module Completion Order

1. Foundation (auth, users, branches, settings, audit)
2. Catalog (categories, manufacturers, units, medicines)
3. Partners (suppliers, customers)
4. Inventory (batches, movements, adjustments, transfers)
5. Purchasing (orders, receipts)
6. Sales/POS (FEFO, payments, invoices, cancellation)
7. Prescriptions
8. Returns/refunds
9. Expenses
10. Employees
11. Dashboard
12. Reports & exports
13. UI polish, tests, docs
