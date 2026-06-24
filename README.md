# InfiniteJourney SaaS Platform

Enterprise multi-tenant platform for non-profits, Islamic organizations, charities, and community groups.

## Three deployable projects

Each project is **independent** — own Docker, own README, deploy separately:

| Project | Purpose | Local start |
|---------|---------|-------------|
| [InfiniteJourney.Keycloak](InfiniteJourney.Keycloak/) | Identity (OIDC/JWT) | `docker compose up -d` |
| [InfiniteJourney.Backend](InfiniteJourney.Backend/) | API + PostgreSQL + Redis | `docker compose up -d` |
| [InfiniteJourney.Frontend](InfiniteJourney.Frontend/) | Angular SPA (nginx) | `docker compose up -d --build` |

**Full setup guide:** [docs/SETUP.md](docs/SETUP.md)  
**Architecture & IAM flow:** [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)

---

## Quick start (local development)

### Option A — All services at once

```powershell
docker compose -f docker-compose.dev.yml up -d
```

### Option B — Step by step (recommended while learning)

```powershell
# 1. Identity
cd InfiniteJourney.Keycloak
copy .env.example .env
docker compose up -d

# 2. Backend (PostgreSQL + API)
cd ../InfiniteJourney.Backend
copy .env.example .env
docker compose up -d postgres redis
dotnet run --project Web/InfiniteJourney.Web

# 3. Frontend
cd ../InfiniteJourney.Frontend/Web/InfiniteJourney.Web
npm install
npm start
```

### Open

| URL | Service |
|-----|---------|
| http://hope.localhost:4200 | Frontend (Hope tenant) |
| http://hope.localhost:5274/api/campaigns | Backend API |
| http://localhost:5274/swagger | API docs |
| http://localhost:8080 | Keycloak admin |

**Test user:** `admin@hope.org` / `Password123!`

---

## Repository layout

```
infinite-journey-saas-app/
├── InfiniteJourney.Keycloak/     # Realm, theme, Keycloak Docker
├── InfiniteJourney.Backend/      # .NET 9 Clean Architecture API
├── InfiniteJourney.Frontend/     # Angular 21 SPA
├── docs/                         # Platform-wide documentation
├── docker-compose.dev.yml        # Optional: run all three locally
├── implementation_plan.md
└── initial_plan.md
```

---

## How Keycloak connects (summary)

1. **Frontend** redirects user to Keycloak login (`infinite-journey-web` client, PKCE)
2. Keycloak returns a **JWT access token** to the browser
3. **Frontend** sends `Authorization: Bearer <token>` on every API call
4. **Backend** validates the JWT against Keycloak's public keys (`Keycloak:Authority`)

No shared secrets between Frontend and Backend — only the issuer URL must match.

---

## Phase 1 status

| Task | Status |
|------|--------|
| T1–T2 Multi-tenancy foundation | Done |
| T3 Keycloak JWT auth | Done |
| T4 Domain model | Done |
| T5 Campaign API | Done |
| T6 NSwag | Done |
| T7 Angular UI | Done |
| Three-project deployment structure | Done |

See `implementation_plan.md` for the full blueprint.

---

## Suggested next move

1. **Donation module** — payment flow + domain events (natural follow-up to Campaigns)
2. **User sync** — create `User` + `Membership` records on first Keycloak login
3. **Production Keycloak** — `start` mode, external DB, TLS, remove dev users
4. **CI/CD per project** — separate pipelines for Keycloak, Backend, Frontend images
