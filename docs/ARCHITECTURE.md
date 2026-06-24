# InfiniteJourney — Platform Architecture

## Three-project deployment model

InfiniteJourney is split into **three independently deployable projects**. Each owns its Docker configuration, environment variables, and lifecycle.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         infinite-journey-saas-app                            │
├─────────────────────┬─────────────────────┬───────────────────────────────┤
│ InfiniteJourney     │ InfiniteJourney     │ InfiniteJourney               │
│ .Keycloak           │ .Backend            │ .Frontend                     │
│                     │                     │                               │
│ • Realm + roles     │ • REST API          │ • Angular SPA                 │
│ • OIDC / JWT issuer │ • PostgreSQL        │ • nginx static hosting        │
│ • Login theme       │ • Multi-tenant EF   │ • Runtime app-config.json     │
│ Port: 8080          │ Port: 5274          │ Port: 4200                    │
└──────────┬──────────┴──────────┬──────────┴───────────────┬───────────────┘
           │                     │                          │
           │    JWT validated    │◄──── Bearer token ───────┘
           └────────────────────►│
                                 │
                    PostgreSQL ◄─┘ (tenant data, not identity)
```

## Identity flow (Keycloak ↔ Frontend ↔ Backend)

### Step 1 — User login (Frontend ↔ Keycloak)

```
Browser                    Keycloak                      Frontend SPA
   │                          │                              │
   │── Sign in click ────────►│                              │
   │◄─ Login page (theme) ────│                              │
   │── credentials ──────────►│                              │
   │◄─ redirect + code ──────│                              │
   │                          │◄── PKCE token exchange ──────│
   │                          │── access_token (JWT) ───────►│
```

- **Client:** `infinite-journey-web` (public, PKCE)
- **Config:** `InfiniteJourney.Frontend` → `assets/app-config.json`

### Step 2 — API call (Frontend ↔ Backend)

```
Frontend                              Backend API
   │                                       │
   │  GET /api/campaigns                   │
   │  Host: hope.localhost:5274            │
   │  Authorization: Bearer eyJhbG...      │
   │──────────────────────────────────────►│
   │                                       │── resolve tenant (host header)
   │                                       │── validate JWT (Keycloak JWKS)
   │                                       │── apply tenant query filter
   │◄──────────────────────────────────────│
```

- **Client:** `infinite-journey-api` (bearer-only — no login UI)
- **Config:** `InfiniteJourney.Backend` → `Keycloak:Authority`

### Step 3 — JWT validation (Backend ↔ Keycloak)

Backend never calls Keycloak on every request. It:

1. Reads `Keycloak:Authority` (e.g. `http://localhost:8080/realms/InfiniteJourney`)
2. Fetches `.well-known/openid-configuration` once (cached)
3. Validates JWT signature using JWKS public keys
4. Reads roles from `realm_access.roles` claim

## Configuration matrix

| Setting | Keycloak project | Backend project | Frontend project |
|---------|------------------|-----------------|------------------|
| Issuer URL | `KC_HOSTNAME` | `Keycloak__Authority` | `keycloak.url` + realm |
| SPA client | realm JSON | — | `keycloak.clientId` |
| API client | realm JSON (bearer) | validates audience optional | — |
| Database | Keycloak DB (future) | PostgreSQL | — |
| Theme | `themes/infinitejourney/` | — | app CSS variables |

## Keycloak theme strategy

| Approach | When to use | This project |
|----------|-------------|--------------|
| **Extend base theme (CSS)** | Brand colors, logo, typography | **Current** — `themes/infinitejourney/` |
| **Keycloakify (React)** | Fully custom login React UI | Future Phase 2 option |
| **Official theme fork** | Not recommended — hard to upgrade | Not used |

Our theme extends the official Keycloak base:

```
themes/infinitejourney/login/theme.properties
  parent=keycloak
  import=common/keycloak
```

No "eject" needed — override CSS only, upgrade Keycloak safely.

## Production deployment pattern

| Environment | Keycloak | Backend | Frontend |
|-------------|----------|---------|----------|
| Dev | `docker compose up` (start-dev) | `dotnet run` or compose | `npm start` or compose |
| Staging | Dedicated VM/K8s | Container + managed PG | CDN / nginx container |
| Production | HA Keycloak cluster + external DB | K8s + RDS | CDN + WAF |

Each project publishes its own container image. URLs connect via environment variables only.

## Local orchestration

```powershell
# From repo root — convenience only, not required for production
docker compose -f docker-compose.dev.yml up -d
```

Individual projects remain fully independent.
