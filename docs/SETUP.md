# InfiniteJourney — Setup Guide

This repository is set up for a straightforward local workflow.

## Prerequisites

Make sure the following are available on your machine:

- Docker Desktop
- .NET SDK
- Node.js

## Start the local apps

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

## Open the app

- Frontend: http://hope.localhost:4200
- Backend API: http://hope.localhost:5274/api/campaigns
- Swagger: http://localhost:5274/swagger
- Keycloak admin: http://localhost:8080

## Default login

- Email: admin@hope.org
- Password: Password123!

## Notes

- No extra `.env` copy step is required for the local flow above.
- The project documentation for architecture and domain design is available in [ARCHITECTURE.md](ARCHITECTURE.md) and [compodoc-additional-docs/03-domain-models.md](compodoc-additional-docs/03-domain-models.md).
