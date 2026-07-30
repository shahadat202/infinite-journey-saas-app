# InfiniteJourney.Backend

ASP.NET Core 9 API for the multi-tenant SaaS platform.

## Start locally

```powershell
cd InfiniteJourney.Backend
dotnet run --project Web/InfiniteJourney.Web
```

Open the API at:

- Swagger: http://localhost:5274/swagger
- Tenant API: http://hope.localhost:5274/api/campaigns

No helper scripts are required for this local flow.

## Development notes

- The API is expected to be run alongside the local Keycloak instance.
- Database migrations are applied automatically on startup.
- For more setup details, see [../docs/SETUP.md](../docs/SETUP.md).
