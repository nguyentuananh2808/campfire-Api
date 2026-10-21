# Backend instructions

- Follow root `AGENTS.md` and the dependency graph documented in `docs/architecture.md`.
- Domain -> nothing; Application -> Domain/Contracts; Infrastructure -> Application/Domain; Presentation -> Application; Host only composes modules.
- Every module owns its PostgreSQL schema, DbContext, migrations and migration history table. Never query across module tables.
- Add migrations with the module's design-time factory and verify through `Campfire.DbMigrator`; integration tests require real PostgreSQL.
- Keep auth cookies and antiforgery enabled for mutations. Redact secrets, invitation tokens and chat bodies from logs.
- Run `dotnet restore --locked-mode`, `dotnet build --no-restore`, `dotnet test --no-build` and `dotnet format --verify-no-changes` from `backend/` when lock files exist.
