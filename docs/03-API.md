# API Endpoint Plan

Base URL: `http://localhost:5098/api` (default http launch profile; the WinForms client reads this from `appsettings.json`)
All endpoints except `POST /auth/login`, `POST /auth/refresh` and `GET /health` require a valid JWT.
Every endpoint is additionally protected by a permission policy noted in the table.

## Auth

| Method | Route | Permission | Description |
|---|---|---|---|
| POST | `/auth/login` | anonymous | Returns access token, refresh token, expiry, user profile and permissions |
| POST | `/auth/refresh` | anonymous | Rotates refresh token, returns new token pair |
| POST | `/auth/logout` | authenticated | Revokes the supplied refresh token |
| GET | `/auth/me` | authenticated | Current user profile, roles, permissions, branch |
| POST | `/auth/change-password` | authenticated | Change own password |

## Users, Roles, Branches, Employees

| Method | Route | Permission |
|---|---|---|
| GET | `/users` | `Users.View` |
| GET | `/users/{id}` | `Users.View` |
| POST | `/users` | `Users.Manage` |
| PUT | `/users/{id}` | `Users.Manage` |
| POST | `/users/{id}/reset-password` | `Users.Manage` |
| POST | `/users/{id}/toggle-active` | `Users.Manage` |
| GET | `/users/roles` | `Users.View` |
| GET | `/users/permissions` | `Users.View` |
| GET | `/branches` | `Branches.View` |
| POST/PUT/DELETE | `/branches/{id}` | `Branches.Manage` |
| GET | `/employees` | `Employees.View` |
| GET | `/employees/{id}` | `Employees.View` |
| POST/PUT/DELETE | `/employees` | `Employees.Manage` |
| GET | `/employees/{id}/activity` | `Employees.View` |

## Catalog

| Method | Route | Permission |
|---|---|---|
| GET | `/medicine-categories` | `Medicines.View` |
| POST/PUT/DELETE | `/medicine-categories` | `Medicines.Manage` |
| GET | `/manufacturers` | `Medicines.View` |
| POST/PUT/DELETE | `/manufacturers` | `Medicines.Manage` |
| GET | `/units` | `Medicines.View` |
| POST/PUT/DELETE | `/units` | `Medicines.Manage` |

## Medicines

| Method | Route | Permission | Description |
|---|---|---|---|
| GET | `/medicines` | `Medicines.View` | Paged, search, category/manufacturer/status filters, lowStock flag |
| GET | `/medicines/{id}` | `Medicines.View` | Details incl. stock totals |
| GET | `/medicines/barcode/{barcode}` | `Medicines.View` | Barcode lookup |
| GET | `/medicines/lookup?q=` | `Medicines.View` | POS lookup by name/barcode/SKU with sellable batches |
| GET | `/medicines/low-stock` | `Inventory.View` | Low stock list |
| GET | `/medicines/expiring?days=` | `Inventory.View` | Expiring batches |
| GET | `/medicines/expired` | `Inventory.View` | Expired batches |
| POST | `/medicines` | `Medicines.Manage` | Create |
| PUT | `/medicines/{id}` | `Medicines.Manage` | Update |
| DELETE | `/medicines/{id}` | `Medicines.Manage` | Soft delete/deactivate |
| GET | `/medicines/{id}/batches` | `Inventory.View` | Batch list |
| GET | `/medicines/{id}/stock-history` | `Inventory.View` | Inventory movements |

## Inventory

| Method | Route | Permission | Description |
|---|---|---|---|
| GET | `/inventory/batches` | `Inventory.View` | Paged batches with filters |
| GET | `/inventory/stock` | `Inventory.View` | Stock summary per medicine |
| GET | `/inventory/movements` | `Inventory.View` | Paged movement history |
| GET | `/inventory/valuation` | `Inventory.View` | Stock valuation |
| GET | `/inventory/alerts/low-stock` | `Inventory.View` | Alerts |
| GET | `/inventory/alerts/expiring` | `Inventory.View` | Alerts |
| GET | `/inventory/alerts/expired` | `Inventory.View` | Alerts |
| POST | `/inventory/stock-in` | `Inventory.Adjust` | New batch stock in |
| POST | `/inventory/stock-out` | `Inventory.Adjust` | Stock out (damage/expiry/manual) |
| POST | `/inventory/adjustments` | `Inventory.Adjust` | Set/adjust batch quantity with reason |

## Stock Transfers

| Method | Route | Permission |
|---|---|---|
| GET | `/stock-transfers` | `Inventory.View` |
| GET | `/stock-transfers/{id}` | `Inventory.View` |
| POST | `/stock-transfers` | `Inventory.Transfer` |
| POST | `/stock-transfers/{id}/approve` | `Inventory.Transfer` |
| POST | `/stock-transfers/{id}/complete` | `Inventory.Transfer` |
| POST | `/stock-transfers/{id}/cancel` | `Inventory.Transfer` |

## Suppliers & Customers

| Method | Route | Permission |
|---|---|---|
| GET | `/suppliers` | `Suppliers.View` |
| GET | `/suppliers/{id}` | `Suppliers.View` |
| POST/PUT/DELETE | `/suppliers` | `Suppliers.Manage` |
| GET | `/suppliers/{id}/purchases` | `Suppliers.View` |
| GET | `/suppliers/{id}/payments` | `Suppliers.View` |
| GET | `/suppliers/{id}/statement` | `Suppliers.View` |
| POST | `/suppliers/{id}/payments` | `Suppliers.Manage` |
| GET | `/customers` | `Customers.View` |
| GET | `/customers/{id}` | `Customers.View` |
| POST/PUT/DELETE | `/customers` | `Customers.Manage` |
| GET | `/customers/{id}/purchases` | `Customers.View` |
| GET | `/customers/{id}/payments` | `Customers.View` |
| GET | `/customers/{id}/statement` | `Customers.View` |
| POST | `/customers/{id}/payments` | `Customers.Manage` |

## Purchasing

| Method | Route | Permission | Description |
|---|---|---|---|
| GET | `/purchase-orders` | `Purchases.View` | Paged with status/supplier/date filters |
| GET | `/purchase-orders/{id}` | `Purchases.View` | Details |
| POST | `/purchase-orders` | `Purchases.Create` | Create draft |
| PUT | `/purchase-orders/{id}` | `Purchases.Create` | Update draft |
| POST | `/purchase-orders/{id}/submit` | `Purchases.Create` | Submit for approval |
| POST | `/purchase-orders/{id}/approve` | `Purchases.Approve` | Approve |
| POST | `/purchase-orders/{id}/reject` | `Purchases.Approve` | Reject |
| POST | `/purchase-orders/{id}/receive` | `Purchases.Receive` | Receive goods (partial allowed) |
| POST | `/purchase-orders/{id}/cancel` | `Purchases.Approve` | Cancel |
| GET | `/goods-receipts` | `Purchases.View` | Receipt history |
| GET | `/goods-receipts/{id}` | `Purchases.View` | Receipt details |

## Sales / POS

| Method | Route | Permission | Description |
|---|---|---|---|
| GET | `/sales` | `Sales.View` | Paged sales with filters |
| GET | `/sales/{id}` | `Sales.View` | Full sale incl. details/payments |
| POST | `/sales` | `Sales.Create` | Complete sale: FEFO, stock, payments, invoice, audit |
| POST | `/sales/{id}/cancel` | `Sales.Cancel` | Cancel with authorization, restore eligible stock |
| GET | `/sales/{id}/invoice` | `Sales.View` | Invoice data for printing |
| POST | `/sales/{id}/payments` | `Sales.Create` | Record settlement payment for credit sale |
| GET | `/sales/today-summary` | `Sales.View` | Today's totals/payment breakdown |

## Prescriptions

| Method | Route | Permission |
|---|---|---|
| GET | `/prescriptions` | `Prescriptions.View` |
| GET | `/prescriptions/{id}` | `Prescriptions.View` |
| POST | `/prescriptions` | `Prescriptions.Create` |
| PUT | `/prescriptions/{id}` | `Prescriptions.Create` |
| POST | `/prescriptions/{id}/verify` | `Prescriptions.Verify` |
| POST | `/prescriptions/{id}/reject` | `Prescriptions.Verify` |
| POST | `/prescriptions/{id}/image` | `Prescriptions.Create` |

## Returns & Refunds

| Method | Route | Permission | Description |
|---|---|---|---|
| GET | `/returns` | `Returns.View` | Paged |
| GET | `/returns/{id}` | `Returns.View` | Details incl. refunds |
| POST | `/returns` | `Returns.Create` | Create sale/purchase return (Pending) |
| POST | `/returns/{id}/approve` | `Returns.Approve` | Inspect, restock if eligible, refund |
| POST | `/returns/{id}/reject` | `Returns.Approve` | Reject with reason |
| GET | `/returns/{id}/refund` | `Returns.View` | Refund record |

## Expenses

| Method | Route | Permission |
|---|---|---|
| GET | `/expenses` | `Expenses.View` |
| GET | `/expenses/{id}` | `Expenses.View` |
| POST/PUT/DELETE | `/expenses` | `Expenses.Manage` |
| POST | `/expenses/{id}/attachment` | `Expenses.Manage` |
| GET/POST/PUT/DELETE | `/expense-categories` | `Expenses.Manage` |

## Dashboard & Reports

| Method | Route | Permission |
|---|---|---|
| GET | `/dashboard` | `Dashboard.View` |
| GET | `/dashboard/sales-chart?days=` | `Dashboard.View` |
| GET | `/dashboard/top-selling?count=` | `Dashboard.View` |
| GET | `/reports/sales?from=&to=&groupBy=` | `Reports.View` |
| GET | `/reports/profit-loss?from=&to=` | `Reports.View` |
| GET | `/reports/purchases?from=&to=` | `Reports.View` |
| GET | `/reports/inventory` | `Reports.View` |
| GET | `/reports/stock-valuation` | `Reports.View` |
| GET | `/reports/expiry?days=` | `Reports.View` |
| GET | `/reports/low-stock` | `Reports.View` |
| GET | `/reports/suppliers?from=&to=` | `Reports.View` |
| GET | `/reports/customers?from=&to=` | `Reports.View` |
| GET | `/reports/expenses?from=&to=` | `Reports.View` |
| GET | `/reports/returns?from=&to=` | `Reports.View` |
| GET | `/reports/payments?from=&to=` | `Reports.View` |
| GET | `/reports/employee-activity?from=&to=` | `Reports.View` |
| GET | `/reports/export?type=&format=excel\|pdf&from=&to=` | `Reports.Export` |

## Settings, Audit, Health

| Method | Route | Permission |
|---|---|---|
| GET | `/settings` | `Settings.View` |
| PUT | `/settings` | `Settings.Manage` |
| GET | `/audit-logs` | `Audit.View` |
| GET | `/audit-logs/{id}` | `Audit.View` |
| GET | `/health` | anonymous |

## Error Contract

Problem Details (RFC 7807):

```json
{
  "type": "https://httpstatuses.com/400",
  "title": "Validation failed",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "errors": { "quantity": ["Quantity must be greater than zero."] },
  "traceId": "..."
}
```

- 400 validation/business rule, 401 unauthenticated, 403 forbidden, 404 not found, 409 conflict/concurrency, 500 unexpected.
