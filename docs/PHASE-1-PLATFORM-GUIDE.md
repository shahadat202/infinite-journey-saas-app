# InfiniteJourney — Phase 1 Platform Guide

> **Purpose:** Single reference for everything implemented in Phase 1, how the five platform concerns work together, how to run and test them, and what remains for Phase 2.

---

## Table of contents

1. [Platform overview](#1-platform-overview)
2. [Tenant, User & Membership](#2-tenant-user--membership)
3. [Global paginated grid (GetAll)](#3-global-paginated-grid-getall)
4. [Global error handling](#4-global-error-handling)
5. [Compodoc (frontend documentation)](#5-compodoc-frontend-documentation)
6. [Campaign CRUD & file upload](#6-campaign-crud--file-upload)
7. [Run & test checklist](#7-run--test-checklist)
8. [File map (what was added)](#8-file-map-what-was-added)
9. [Completed vs pending](#9-completed-vs-pending)
10. [Recommended next steps](#10-recommended-next-steps)

---

## 1. Platform overview

InfiniteJourney is an enterprise multi-tenant SaaS platform for nonprofits, charities, and Islamic organizations.

```
infinite-journey-saas-app/
├── InfiniteJourney.Keycloak/       # Identity (OIDC/JWT, realm, custom theme)
├── InfiniteJourney.Backend/        # .NET 9 Clean Architecture API
├── InfiniteJourney.Frontend/       # Angular 21 SPA
│   └── Web/InfiniteJourney.Web/
├── docs/                           # Architecture & guides
└── docker-compose.dev.yml            # Optional local orchestrator
```

### Request flow (simplified)

```
Browser (hope.localhost:4200)
    │
    ├─► Keycloak (login / JWT)
    │
    └─► Backend API (hope.localhost:5274)
            │
            ├─ TenantResolutionMiddleware  → resolves subdomain → TenantId
            ├─ JWT Auth                    → roles from Keycloak
            ├─ Global query filters        → data scoped per tenant
            └─ Controllers / CQRS          → Campaigns, Files, …
```

### Local URLs

| Service   | URL |
|-----------|-----|
| Frontend  | http://hope.localhost:4200 |
| API       | http://hope.localhost:5274 |
| Swagger   | http://localhost:5274/swagger |
| Keycloak  | http://localhost:8080 |

**Test account:** `admin@hope.org` / `Password123!`

---

## 2. Tenant, User & Membership

### Concepts

| Concept | Meaning | Example |
|---------|---------|---------|
| **Tenant** | An organization on the platform | Hope Foundation → subdomain `hope` |
| **User** | One person, one Keycloak login (platform-wide) | Sarah signs in once |
| **Membership** | Link between User and Tenant with a role | Sarah is `OrganizationAdmin` at Hope |

Each tenant has isolated data (campaigns, donations, theme), its own subdomain, and enabled modules.

### How OrganizationZ would onboard (production flow — planned)

```
1. Platform provisions Tenant (subdomain: organizationz)
2. Modules + default theme activated
3. Founder signs up via Keycloak
4. Membership created → OrganizationOwner at organizationz
5. Founder invites staff → more Memberships
```

### Current dev state

- Tenants `hope` and `relief` are **seeded** in the database.
- Subdomain routing works (`hope.localhost`, `relief.localhost`).
- Keycloak login works; JWT is sent on API calls.
- **Not yet implemented:** tenant self-registration API, automatic User/Membership sync on first Keycloak login.

**Detailed guide:** [docs/04-tenant-user-membership-guide.md](./04-tenant-user-membership-guide.md)

---

## 3. Global paginated grid (GetAll)

Every list endpoint should follow the same pattern: **search, sort, pagination** via query string.

### Backend

**Models**

| File | Role |
|------|------|
| `Application/Common/Models/GridQuery.cs` | `pageIndex`, `pageSize`, `search`, `sortBy`, `sortDirection` |
| `Application/Common/Models/PagedResult.cs` | `{ data, pageIndex, pageSize, total }` |
| `Application/Common/Extensions/QueryableGridExtensions.cs` | `ApplySearch`, `ApplySort`, `ToPagedResultAsync` |

**Example — Campaigns**

```
GET /api/campaigns?pageIndex=0&pageSize=10&search=water&sortBy=title&sortDirection=asc&status=Active
```

Response:

```json
{
  "data": [ { "id": "...", "title": "...", "status": "Active", ... } ],
  "pageIndex": 0,
  "pageSize": 10,
  "total": 42
}
```

Sortable columns for campaigns: `title`, `targetamount`, `raisedamount`, `status`, `createdat`.

### Frontend

| File | Role |
|------|------|
| `core/models/grid.model.ts` | `GridQuery`, `PagedResult`, `GridColumn` |
| `shared/components/data-grid/` | Reusable grid: search, sort, pagination, row actions |
| `core/services/campaign-api.service.ts` | HTTP client with paged `getPaged()` |

**Used in:** `/campaigns/manage` (admin grid).

**Reuse pattern for future modules (Donations, Users, etc.):**

1. Backend: query inherits `GridQuery`, handler uses `QueryableGridExtensions`.
2. Frontend: pass columns + `PagedResult` into `<app-data-grid>`.

---

## 4. Global error handling

### Backend

| File | Role |
|------|------|
| `Application/Common/Exceptions/AppExceptions.cs` | `NotFoundException`, `BusinessRuleException`, `ForbiddenAppException` |
| `Application/Common/Models/ApiErrorResponse.cs` | Standard error JSON shape |
| `Web/Middleware/GlobalExceptionHandler.cs` | Maps exceptions → HTTP status + `ApiErrorResponse` |

**Error response shape:**

```json
{
  "statusCode": 404,
  "message": "Campaign {id} was not found.",
  "errorCode": "NOT_FOUND",
  "traceId": "...",
  "errors": []
}
```

Registered in `Program.cs`:

```csharp
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
app.UseExceptionHandler();
```

### Frontend

| File | Role |
|------|------|
| `core/models/api-error.model.ts` | Matches backend `ApiErrorResponse` |
| `core/interceptors/error.interceptor.ts` | Parses API errors, shows toast |
| `core/services/toast.service.ts` | Signal-based toast queue |
| `shared/components/toast/` | Bottom-right toast UI |

Wired in `app.config.ts`:

```typescript
provideHttpClient(withInterceptors([authInterceptor, errorInterceptor]))
```

Toasts auto-dismiss after 5 seconds. Click to dismiss early.

---

## 5. Compodoc (frontend documentation)

Compodoc generates living documentation from Angular source + additional markdown.

### Configuration

- Config: `InfiniteJourney.Frontend/Web/InfiniteJourney.Web/compodoc.json`
- Extra docs: `docs/compodoc-additional-docs/` (Keycloak, domain models)

### Commands

```powershell
cd InfiniteJourney.Frontend/Web/InfiniteJourney.Web

# Build static docs → dist-docs/
npm run docs:build

# Build + serve at http://localhost:8085
npm run docs:serve
```

`npm run present` is an alias for `docs:serve`.

---

## 6. Campaign CRUD & file upload

### Backend API

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `/api/campaigns` | Public | Paged list (optional `status` filter) |
| GET | `/api/campaigns/{id}` | Public | Detail |
| POST | `/api/campaigns` | TenantStaff | Create (Draft) |
| PUT | `/api/campaigns/{id}` | TenantStaff | Update |
| DELETE | `/api/campaigns/{id}` | TenantStaff | Delete (blocked if `raisedAmount > 0`) |
| POST | `/api/campaigns/{id}/activate` | TenantStaff | Draft → Active |
| POST | `/api/files/upload` | TenantStaff | Base64 file upload |

### File storage

- Service: `LocalFileStorageService`
- Root: `UPLOADED_DATA/{tenantId}/images|pdfs/` (configurable via `Storage:RootPath`)
- Served at: `/uploads/...` (static files middleware)
- Upload request body: `{ fileName, contentType, base64Data, category }`
- Returns: `{ path, fileName, contentType, sizeBytes }` — store **path** in campaign `coverImageUrl`, not base64.

### Frontend routes

| Route | Page | Auth |
|-------|------|------|
| `/campaigns` | Public campaign cards (Active only) | No |
| `/campaigns/:id` | Campaign detail | No |
| `/campaigns/manage` | Admin grid + create/edit panel | Yes (Keycloak) |

### Frontend services

| Service | Role |
|---------|------|
| `CampaignApiService` | CRUD + paged list + upload |
| `FileUploadService` | Image compression + base64 upload |

### Admin workflow

1. Sign in → header shows **Manage** link.
2. Open `/campaigns/manage` → searchable/sortable grid.
3. **New campaign** → slide-over form, optional cover image upload.
4. **Save** → creates Draft campaign.
5. **Activate** → publishes campaign (visible on public list).
6. **Edit / Delete** from grid row actions.

---

## 7. Run & test checklist

### Start stack

```powershell
# 1. Keycloak
cd InfiniteJourney.Keycloak
docker compose up -d

# 2. Backend (includes Postgres via docker or local)
cd InfiniteJourney.Backend
docker compose up -d postgres redis   # if using docker DB
dotnet run --project Web/InfiniteJourney.Web

# 3. Frontend
cd InfiniteJourney.Frontend/Web/InfiniteJourney.Web
npm install
npm start
```

### Manual tests

| # | Test | Expected |
|---|------|----------|
| 1 | Open http://hope.localhost:4200/campaigns | Active campaigns listed |
| 2 | Click a campaign | Detail with progress bar |
| 3 | Sign in as admin@hope.org | Header shows user + Manage |
| 4 | Open /campaigns/manage | Paged grid loads |
| 5 | Create campaign + upload image | Toast success; appears in grid as Draft |
| 6 | Activate campaign | Status Active; visible on public list |
| 7 | Edit campaign title | Toast success; grid updates |
| 8 | Delete empty campaign | Removed from grid |
| 9 | Trigger error (bad ID) | Red toast bottom-right |
| 10 | API paged query | `GET .../api/campaigns?pageIndex=0&pageSize=5` returns paged JSON |

### Regenerate NSwag client (optional)

Backend build auto-runs NSwag. To regenerate manually from frontend:

```powershell
# Backend must be running
cd InfiniteJourney.Frontend/Web/InfiniteJourney.Web
npm run generate-api
```

> Note: Public pages now use `CampaignApiService` (HttpClient). NSwag client remains available for future use.

---

## 8. File map (what was added)

### Backend — Application layer

```
Application/Common/Models/GridQuery.cs
Application/Common/Models/PagedResult.cs
Application/Common/Models/ApiErrorResponse.cs
Application/Common/Extensions/QueryableGridExtensions.cs
Application/Common/Exceptions/AppExceptions.cs
Application/Common/Interfaces/IFileStorageService.cs
Application/Files/Commands/UploadFileCommand.cs
Application/Campaigns/Commands/UpdateCampaignCommandHandler.cs
Application/Campaigns/Commands/DeleteCampaignCommandHandler.cs
Application/Campaigns/Queries/GetCampaignsQueryHandler.cs  (updated)
```

### Backend — Infrastructure & Web

```
Infrustructure/Storage/LocalFileStorageService.cs
Infrustructure/Storage/StorageOptions.cs
Web/Middleware/GlobalExceptionHandler.cs
Web/Controllers/FilesController.cs
Web/Controllers/CampaignsController.cs  (PUT, DELETE, paged GET)
```

### Backend — Domain

```
Domain/Aggregates/Campaign/Campaign.cs  (EnsureCanDelete, UpdateDetails)
```

### Frontend — Core

```
core/models/grid.model.ts
core/models/api-error.model.ts
core/models/campaign.model.ts
core/services/toast.service.ts
core/services/campaign-api.service.ts
core/services/file-upload.service.ts
core/interceptors/error.interceptor.ts
core/guards/auth.guard.ts
```

### Frontend — Shared & Features

```
shared/components/toast/
shared/components/data-grid/
features/campaigns/pages/campaign-admin/
features/campaigns/pages/campaign-list/     (updated — paged API)
features/campaigns/pages/campaign-detail/     (updated — CampaignApiService)
app.config.ts, app.routes.ts, app.html, app.ts  (wired)
```

### Documentation

```
docs/04-tenant-user-membership-guide.md
docs/PHASE-1-PLATFORM-GUIDE.md   ← this file
docs/compodoc-additional-docs/
```

---

## 9. Completed vs pending

### Completed in Phase 1

- [x] Multi-tenant resolution (subdomain → TenantId)
- [x] EF Core global query filters + save interceptor
- [x] Keycloak JWT authentication
- [x] Domain model (Tenant, User, Membership, Campaign, Donation, Theme, ModuleActivation)
- [x] Campaign CQRS (Create, Read, Update, Delete, Activate)
- [x] Paged grid pattern (backend + reusable frontend grid)
- [x] Global exception handler + frontend error toasts
- [x] Local file upload (images/PDFs) with tenant isolation
- [x] Campaign admin UI (manage grid + form)
- [x] Public campaign list + detail
- [x] Compodoc scripts (`docs:build`, `docs:serve`)
- [x] Three-project structure (Keycloak / Backend / Frontend)

### Pending (Phase 2+)

- [ ] Tenant self-registration / onboarding API
- [ ] User + Membership sync on first Keycloak login
- [ ] Donations module (create, payment integration)
- [ ] Role-based UI (hide Manage for non-staff)
- [ ] NSwag client regen alignment (optional — HttpClient service is primary)
- [ ] Production Keycloak + HTTPS + object storage (S3/Azure Blob instead of local files)
- [ ] E2E tests (Playwright/Cypress)
- [ ] CI pipeline (build, test, deploy)

---

## 10. Recommended next steps

1. **User sync middleware** — on first authenticated request, upsert `User` + `Membership` from JWT claims.
2. **Donations module** — domain entity already exists; add CQRS + public donate flow.
3. **Extend grid pattern** — apply `GridQuery` + `DataGridComponent` to donations admin.
4. **Production storage** — replace `LocalFileStorageService` with cloud blob storage behind `IFileStorageService`.
5. **Tenant onboarding** — API + Stripe checkout for new organizations.

---

## Related documentation

| Document | Description |
|----------|-------------|
| [ARCHITECTURE.md](./ARCHITECTURE.md) | Full platform architecture |
| [SETUP.md](./SETUP.md) | Environment setup |
| [04-tenant-user-membership-guide.md](./04-tenant-user-membership-guide.md) | Tenant/User/Membership deep dive |
| [compodoc-additional-docs/03-domain-models.md](./compodoc-additional-docs/03-domain-models.md) | Entity glossary |

---

*Last updated: Phase 1 completion — grid, errors, CRUD, file upload, admin UI, and this guide.*
