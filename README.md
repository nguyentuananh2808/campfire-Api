# Campfire

Campfire is a private, mobile-first space for close friend groups. The repository is being built as a .NET 10 modular monolith, Angular 22 SPA and PostgreSQL 18 database.

> Current state: Identity, Groups, text Chat, Events/RSVP, Stranger Chat and the playable Werewolf slice are implemented through Angular, MediatR/Application/Infrastructure and module-owned PostgreSQL migrations. Docker can build the API/migrator image and run it with PostgreSQL; real media and browser acceptance remain pending. See `docs/progress.md`.

## Prerequisites

- .NET SDK 10.0.112 (a repository-local copy can live in `.dotnet/`)
- Node 24.18+ and npm 11.16+
- Docker Desktop/Engine with Compose

## Build locally

PowerShell:

```powershell
$env:DOTNET_CLI_HOME = "$PWD/.dotnet-home"
$env:NUGET_PACKAGES = "$PWD/.nuget/packages"
./.dotnet/dotnet.exe restore backend/Campfire.slnx --locked-mode
./.dotnet/dotnet.exe build backend/Campfire.slnx --no-restore --disable-build-servers -m:1
Set-Location frontend
npm ci
npm run build
npm test -- --watch=false
```

When a system .NET 10 SDK is installed, replace `./.dotnet/dotnet.exe` with `dotnet`.

## Run the application locally

Copy `.env.example` to `.env`, replace the local PostgreSQL password, then run:

```powershell
docker compose --env-file .env -f infra/compose.yml up -d postgres livekit
$env:ConnectionStrings__Campfire = "Host=localhost;Port=5433;Database=campfire;Username=campfire;Password=<your-local-password>"
$env:LiveKit__PublicUrl = "ws://localhost:7880"
$env:LiveKit__ApiUrl = "http://localhost:7880"
$env:LiveKit__ApiKey = "<same-as-LIVEKIT_API_KEY-in-.env>"
$env:LiveKit__ApiSecret = "<same-32-byte-minimum-secret-as-.env>"
./.dotnet/dotnet.exe run --project backend/tools/Campfire.DbMigrator
./.dotnet/dotnet.exe run --project backend/src/Host/Campfire.Api --launch-profile http
```

In another terminal:

```powershell
Set-Location frontend
npm start
```

Open `http://localhost:4200`. Angular proxies `/api` to the API on `http://localhost:5080`. Do not use `docker compose down -v` unless you deliberately intend to erase local data.

To build and run the API plus the one-shot migrator in Docker, use:

```powershell
docker compose --env-file .env -f infra/compose.yml up -d --build api
docker compose --env-file .env -f infra/compose.yml ps -a
```

The local LAN mapping currently binds the container only to `http://192.168.5.185:92`; Swagger is available at `http://192.168.5.185:92/swagger/index.html` while the container uses the Development environment. Change the explicit host mapping in `infra/compose.yml` when the Wi-Fi address changes. Opening TCP 92 to other devices requires an Administrator-created Windows Firewall rule for the Private network profile.

LiveKit receives audio only; this configuration does not enable recording. LAN/Internet multi-device use additionally requires a reachable public WebSocket URL, TLS and TURN/UDP ports as described in `docs/operations.md`.

## Documentation

Start with `docs/architecture.md`, `docs/implementation-plan.md`, `docs/progress.md` and `docs/verification.md`. The source specification is `Campfire-Codex-Bootstrap.md`.
