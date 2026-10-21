# Campfire API

Campfire API is a .NET 10 modular monolith backed by PostgreSQL 18. Each module owns its Domain, Application, Infrastructure, Presentation, Contracts, database schema and migrations. HTTP use cases follow `endpoint -> ISender -> handler -> Application interface -> Infrastructure implementation`.

## Build

```powershell
.\.dotnet\dotnet.exe restore backend\Campfire.slnx --locked-mode
.\.dotnet\dotnet.exe build backend\Campfire.slnx -c Release --no-restore -m:1
```

When the repository-local SDK is not present, use a compatible .NET 10 SDK through `dotnet`.

## Run with Docker

Copy `.env.example` to `.env`, replace every placeholder, then run:

```powershell
docker compose --env-file .env -f infra/compose.yml up -d --build api
docker compose --env-file .env -f infra/compose.yml ps -a
```

The Compose file runs PostgreSQL, applies every module migration through the one-shot migrator, and then starts the API. The current LAN Development mapping is `http://192.168.5.185:92`; change the explicit host address in `infra/compose.yml` when the machine address changes.

Health endpoints are `/health/live` and `/health/ready`. Swagger is available at `/swagger/index.html` only in Development.

Do not commit `.env`, credentials, Data Protection keys or database volumes. The Development LAN configuration is not an Internet-facing production deployment.
