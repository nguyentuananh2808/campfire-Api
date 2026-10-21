# Campfire — Agent Instructions

## Product and scope
- Private friend-group app. Current MVP: Identity, Groups, text Chat, Events/RSVP.
- Funds, Plans, Werewolf, Voice are roadmap only unless explicitly requested.
- Read `Campfire-Codex-Bootstrap.md`, `docs/architecture.md`, `docs/implementation-plan.md` and `docs/progress.md` before substantial changes.
- Treat implemented code, current tests and accepted ADRs as the current state; reconcile stale docs explicitly.

## Architecture
- One deployable .NET backend, modular monolith; Angular SPA; PostgreSQL.
- Each module owns Domain, Application, Infrastructure, Presentation, Contracts and its database schema/DbContext.
- Domain has no framework/infrastructure dependency. Application has no Infrastructure/Presentation dependency.
- Every module dispatches use cases through `API endpoint -> ISender (MediatR) -> command/query handler -> injected Application interface -> Infrastructure implementation -> EF Core/PostgreSQL or the module's framework adapter`.
- Presentation endpoints never call handlers, DbContexts or framework services directly. Handlers never reference Infrastructure types.
- Cross-module references only through Contracts. Never access another module's DbContext/entities/tables.
- Host is composition only. Keep BuildingBlocks minimal; no business entities in shared code.
- One top-level type per file; separate commands, handlers, validators, DTOs, endpoints and EF mappings.
- Angular components use separate TypeScript/template/style files; organize by feature and lazy routes.

## Correctness and security
- Authorization is server-side and resource/group-scoped, including SignalR subscriptions.
- Same-origin Identity cookies and antiforgery protect HTTP mutations. No auth tokens in localStorage.
- Never log passwords, cookies, authorization headers, invite tokens, chat bodies or private game roles.
- Validate in Application; preserve Domain invariants and database constraints under concurrency.
- PostgreSQL migrations per module; UTC timestamps; bounded pagination; cancellation propagated.
- Persist chat before broadcast; use idempotent clientMessageId; reconnect reconciles from database.
- Membership removal revokes realtime access. Use ProblemDetails and structured logs with traceId.
- Keep secrets out of git. No paid AWS provisioning, public publishing or remote push unless requested.

## Working method
- Inspect the repository and existing changes before editing; preserve user work.
- Prefer rg/targeted reads. Verify official docs before introducing dependencies or unfamiliar APIs.
- Implement complete vertical slices, run relevant checks, then update progress/docs.
- Ask only for material ambiguity, destructive actions, credentials or paid external actions.
- Never delete volumes, rewrite history, disable tests/security, or introduce microservices/brokers/Redis/Kubernetes without a requirement and ADR.

## Verification and handoff
- Run format/build, relevant unit/integration/architecture tests and frontend lint/tests/build.
- Use real PostgreSQL for integration tests; run Playwright when browser/runtime is available.
- Record exact results and blockers in `docs/verification.md`; never mark unexecuted checks passed.
- Update `docs/progress.md` with the next actionable step before stopping.
- Final response in Vietnamese: outcome, run instructions, checks and limitations. Code identifiers remain English.
