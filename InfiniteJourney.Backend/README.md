# InfiniteJourney.Backend

ASP.NET Core 9 API — multi-tenant SaaS backend (Clean Architecture).

## Stack

| Layer | Project |
|-------|---------|
| Web | `Web/InfiniteJourney.Web` |
| Application | `Application/InfiniteJourney.Application` |
| Domain | `Domain/InfiniteJourney.Domain` |
| Infrastructure | `Infrustructure/InfiniteJourney.Infrustructure` |

## Dependencies (external services)

| Service | Project | Connection |
|---------|---------|------------|
| PostgreSQL | This compose file | `ConnectionStrings:DefaultConnection` |
| Redis | This compose file | Future tenant cache |
| Keycloak | **InfiniteJourney.Keycloak** (separate deploy) | `Keycloak:Authority` |

## Local development (without Docker API)

```powershell
# 1. Start data stores only
docker compose up -d postgres redis

# 2. Start Keycloak separately
cd ../InfiniteJourney.Keycloak
docker compose up -d

# 3. Run API
dotnet run --project Web/InfiniteJourney.Web
```

API: http://localhost:5274  
Tenant API: http://hope.localhost:5274/api/campaigns  
Swagger: http://localhost:5274/swagger

## Docker (full backend stack)

```powershell
copy .env.example .env
docker compose up -d
```

## Database

Migrations apply automatically on startup. See `../docs/SETUP.md` for pgAdmin connection details.

```powershell
dotnet ef migrations add <Name> `
  --project Infrustructure/InfiniteJourney.Infrustructure `
  --startup-project Web/InfiniteJourney.Web `
  --output-dir Persistence/Migrations
```

## Tests

```powershell
dotnet test
```

## Deploy separately

Build and push the API image. Point `KEYCLOAK_AUTHORITY` to your production Keycloak URL. PostgreSQL can be managed RDS/cloud SQL — remove the `postgres` service from compose in production.
