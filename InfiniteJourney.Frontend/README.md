# InfiniteJourney.Frontend

Angular 21 standalone SPA — tenant-aware public portal and admin UI.

## Structure

```
InfiniteJourney.Frontend/
├── Web/InfiniteJourney.Web/    # Angular application
├── Dockerfile                  # Production nginx image
└── docker-compose.yml          # Standalone web deployment
```

## Dependencies (external services)

| Service | Project | Config key |
|---------|---------|------------|
| Backend API | **InfiniteJourney.Backend** | `apiPort` in app-config |
| Keycloak | **InfiniteJourney.Keycloak** | `keycloak.*` in app-config |

## Local development

```powershell
cd Web/InfiniteJourney.Web
npm install
npm start
```

Open: **http://hope.localhost:4200** (tenant subdomain required)

## Docker (production-like)

```powershell
copy .env.example .env
docker compose up -d --build
```

Open: http://localhost:4200

## Regenerate API client (NSwag)

Backend must be running at http://localhost:5274/swagger

```powershell
cd Web/InfiniteJourney.Web
npm run generate-api
```

## Runtime configuration

`public/assets/app-config.json` is loaded at startup. In Docker, `docker-entrypoint.sh` generates it from environment variables — no rebuild needed when URLs change.

## Deploy separately

Build and push the frontend image. Set `KEYCLOAK_URL` and `API_PORT` env vars to match your deployed Backend and Keycloak URLs.
