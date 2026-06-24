# InfiniteJourney.Keycloak

Standalone identity provider deployment for the InfiniteJourney platform.

## What this project owns

| Asset | Purpose |
|-------|---------|
| `realms/InfiniteJourney-realm.json` | Realm, clients, roles, dev users |
| `themes/infinitejourney/` | Branded login/account UI (extends official Keycloak theme) |
| `docker-compose.yml` | Local/production container for Keycloak only |

## How it connects to Backend and Frontend

```
┌─────────────────────┐         Authorization Code + PKCE          ┌──────────────────────┐
│  InfiniteJourney    │  ───────────────────────────────────────►  │  InfiniteJourney     │
│  .Frontend (SPA)    │         redirects to Keycloak login        │  .Keycloak           │
│  client: web        │  ◄───────────────────────────────────────  │  realm: InfiniteJourney
└─────────┬───────────┘         returns access_token (JWT)         └──────────┬───────────┘
          │                                                                    │
          │  Authorization: Bearer <JWT>                                       │
          │  (every API call)                                                    │
          ▼                                                                    │
┌─────────────────────┐         validates JWT signature                          │
│  InfiniteJourney    │  ──────via JWKS (.well-known/openid-configuration)───────┘
│  .Backend (API)     │
│  client: api        │  bearer-only — never handles login UI
└─────────────────────┘
```

### Clients in this realm

| Client ID | Type | Used by |
|-----------|------|---------|
| `infinite-journey-web` | Public SPA (PKCE) | Angular frontend |
| `infinite-journey-api` | Bearer-only | Backend JWT validation |

### Environment URLs (local defaults)

| Service | URL |
|---------|-----|
| Keycloak Admin | http://localhost:8080 |
| Realm issuer | http://localhost:8080/realms/InfiniteJourney |
| OpenID config | http://localhost:8080/realms/InfiniteJourney/.well-known/openid-configuration |

**Backend** must set:

```json
"Keycloak": {
  "Authority": "http://localhost:8080/realms/InfiniteJourney"
}
```

**Frontend** must set (via `app-config.json` or env):

```json
{
  "keycloak": {
    "url": "http://localhost:8080",
    "realm": "InfiniteJourney",
    "clientId": "infinite-journey-web"
  }
}
```

## Quick start

```powershell
cd InfiniteJourney.Keycloak
copy .env.example .env
docker compose up -d
```

Admin console: http://localhost:8080 — `admin` / value from `.env`

**Dev user:** `admin@hope.org` / `Password123!`

## Custom theme (how it works)

This project uses the **official Keycloak theme extension pattern** — not Keycloakify/React eject:

```
themes/infinitejourney/login/
├── theme.properties    ← parent=keycloak (extends official base)
└── resources/css/      ← your CSS overrides only
```

Keycloak loads themes from `/opt/keycloak/themes/`. Our Docker volume mounts `./themes` there.

To customize further:

1. Edit CSS in `themes/infinitejourney/login/resources/css/login.css`
2. Add images to `resources/img/`
3. Restart Keycloak container

For a **fully custom React login UI**, consider [Keycloakify](https://www.keycloakify.dev/) as a future Phase 2 upgrade — export as a Keycloak theme JAR. The current CSS extension is the recommended starting point aligned with enterprise ops simplicity.

## Production notes

- Replace `start-dev` with `start` and configure TLS (`KC_HTTPS_*`)
- Use external PostgreSQL for Keycloak data (`KC_DB=postgres`)
- Rotate admin password and dev users
- Set `sslRequired: external` in realm for production
- Point Backend `Keycloak__Authority` to your public Keycloak URL

## Deploy separately

This stack is intentionally independent. Deploy Keycloak to its own host/cluster. Backend and Frontend only need the **issuer URL** — no code coupling.
