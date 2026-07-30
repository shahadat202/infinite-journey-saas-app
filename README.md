# InfiniteJourney SaaS Platform

Enterprise multi-tenant platform for nonprofits, Islamic organizations, charities, and community groups.

## Quick start

The local setup is intentionally simple. Start each project directly with its own command.

### 1. Keycloak

```powershell
cd InfiniteJourney.Keycloak
docker compose up -d
```

### 2. Backend

```powershell
cd InfiniteJourney.Backend
dotnet run --project Web/InfiniteJourney.Web
```

### 3. Frontend

```powershell
cd InfiniteJourney.Frontend/Web/InfiniteJourney.Web
npm install
npm start
```

### Open the app

- Frontend: http://hope.localhost:4200
- Backend API: http://hope.localhost:5274/api/campaigns
- Swagger: http://localhost:5274/swagger
- Keycloak admin: http://localhost:8080

### Default test account

- Email: admin@hope.org
- Password: Password123!

> No extra environment-file copy step is required for this local flow.

## Project layout

- [InfiniteJourney.Keycloak](InfiniteJourney.Keycloak/) — Keycloak identity and realm configuration
- [InfiniteJourney.Backend](InfiniteJourney.Backend/) — ASP.NET Core API
- [InfiniteJourney.Frontend](InfiniteJourney.Frontend/) — Angular frontend
- [docs/SETUP.md](docs/SETUP.md) — setup guide
- [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) — architecture overview
