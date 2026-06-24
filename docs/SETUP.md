# InfiniteJourney — Complete Setup Guide

Three **independently deployable** projects. Read this once.

| Project | Folder | What it runs |
|---------|--------|--------------|
| Keycloak | `InfiniteJourney.Keycloak/` | Identity server |
| Backend | `InfiniteJourney.Backend/` | API + PostgreSQL + Redis |
| Frontend | `InfiniteJourney.Frontend/` | Angular SPA |

Architecture diagram: [ARCHITECTURE.md](ARCHITECTURE.md)

---

## Migrations vs pgAdmin (important)

| Action | Result |
|--------|--------|
| `dotnet ef migrations add` | C# files only — **nothing in pgAdmin** |
| Start Backend API once | Creates **tables** in PostgreSQL |
| pgAdmin shows database | Only when Postgres is running + you connect to the **same server** |

You do **not** create tables manually. Create the **database** once (or use Docker), then run the API.

---

## Run order (every dev session)

```
1. InfiniteJourney.Keycloak     →  identity (port 8080)
2. InfiniteJourney.Backend      →  postgres + API (port 5274)
3. InfiniteJourney.Frontend     →  Angular (port 4200)
```

### Option A — All at once (convenience)

```powershell
cd d:\infinite-journey-saas-app
docker compose -f docker-compose.dev.yml up -d
```

Then run Backend locally if you prefer hot reload:

```powershell
cd InfiniteJourney.Backend
dotnet run --project Web/InfiniteJourney.Web
```

### Option B — Step by step (recommended)

#### Step 1 — Keycloak

```powershell
cd InfiniteJourney.Keycloak
copy .env.example .env
docker compose up -d
```

- Admin: http://localhost:8080 (`admin` / `admin`)
- Realm `InfiniteJourney` imports automatically
- Branded login theme: `infinitejourney`
- Test user: `admin@hope.org` / `Password123!`

#### Step 2 — Backend

```powershell
cd InfiniteJourney.Backend
copy .env.example .env
docker compose up -d postgres redis
dotnet run --project Web/InfiniteJourney.Web
```

**pgAdmin connection (Docker Postgres from Backend compose):**

| Field | Value |
|-------|-------|
| Host | `localhost` |
| Port | `5432` |
| Database | `infinite_journey_saas` |
| Username | `postgres` |
| Password | `System@1122` |

Refresh pgAdmin **after** the API runs once — tables appear automatically.

Local PostgreSQL instead? Run `scripts/create-database.sql` in pgAdmin, update `appsettings.Development.json` password.

#### Step 3 — Frontend

```powershell
cd InfiniteJourney.Frontend/Web/InfiniteJourney.Web
npm install
npm start
```

Open: **http://hope.localhost:4200**

---

## How the three projects connect

```
Frontend  ──login──►  Keycloak  ──JWT──►  Frontend  ──Bearer token──►  Backend
                         ▲                                              │
                         └──────── validates JWT signature ─────────────┘
```

| Project | Config | Example |
|---------|--------|---------|
| Keycloak | `.env` | `KC_HOSTNAME=localhost` |
| Backend | `appsettings.json` | `Keycloak:Authority` = `http://localhost:8080/realms/InfiniteJourney` |
| Frontend | `public/assets/app-config.json` | `keycloak.url` = `http://localhost:8080` |

In production, only URLs change — no code changes.

---

## Deploy separately (production)

Each project builds its own Docker image:

```powershell
# Keycloak
cd InfiniteJourney.Keycloak && docker compose up -d

# Backend
cd InfiniteJourney.Backend && docker compose up -d

# Frontend
cd InfiniteJourney.Frontend && docker compose up -d --build
```

Set production URLs via environment variables in each project's `.env`.

---

## NSwag — regenerate API client

```powershell
# Backend must be running
cd InfiniteJourney.Frontend/Web/InfiniteJourney.Web
npm run generate-api
```

---

## Troubleshooting

| Problem | Fix |
|---------|-----|
| No database in pgAdmin | Start Docker Desktop → `cd InfiniteJourney.Backend && docker compose up -d postgres` → run API |
| Tenant API 404 | Use `hope.localhost:5274`, not `localhost:5274` |
| Login fails | Keycloak running? Realm imported? Check http://localhost:8080 |
| CORS errors | Backend `Cors:AllowedOrigins` must include your frontend origin |
| Port 5432 in use | Stop local PostgreSQL or change Backend compose port |

---

## Phase 1 checklist

- [x] Multi-tenancy (T1–T2)
- [x] Keycloak JWT (T3)
- [x] Campaign API (T5)
- [x] Angular UI (T7)
- [x] Three-project deployment structure

**Next:** Donation module → User/Membership sync → Production Keycloak hardening
