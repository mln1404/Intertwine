# Intertwine

Intertwine is a question-based social matching application built with ASP.NET Core, EF Core, Vue, SQL Server, and Redis. Users answer daily and library questions, manage a profile, and spend the application's **Sparks** currency when their free question actions are exhausted.

This repository contains the REST API, Vue frontend, domain and application layers, persistence, background worker, unit tests, and a standalone SOAP payment-provider demonstration.

## Architecture

Intertwine uses a **Clean Architecture-inspired layered design**. The code has clear presentation, application, domain, and infrastructure responsibilities, but it is not strict Clean Architecture: `Services` uses the Identity project, and `Repositories` implements interfaces owned by `Services`.

```text
                           +--------------------+
                           | Intertwine.Web     |
                           | Vue 3 / TypeScript |
                           +---------+----------+
                                     | HTTPS / JSON
                                     v
+----------------+         +---------+----------+
| SOAP demo      |         | Intertwine.API     |
| CoreWCF        |         | REST, JWT, limits |
| standalone     |         +---------+----------+
+----------------+                   |
                                     v
                           +---------+----------+
                           | Services           |
                           | Rules, DTOs, auth  |
                           +---------+----------+
                                     | repository interfaces
                                     v
                           +---------+----------+
                           | Repositories       |
                           | EF Core + Redis    |
                           +----+-----------+---+
                                |           |
                                v           v
                         +------+----+  +---+---+
                         | SQL Server|  | Redis |
                         +-----------+  +-------+
                                ^
                                |
                     +----------+-----------+
                     | Intertwine.Worker    |
                     | Daily assignments    |
                     +----------------------+

Domain contains core entities and enums. Identity contains the ASP.NET
Identity user model. Both are shared by application and infrastructure code.
```

### Project dependencies

```text
API ---------> Repositories -----> Services -----> Domain
                    |                 |
                    +----> Identity <-+
                              |
                              v
                            Domain

Worker ------> Repositories + Services
UnitTests ---> API + Repositories + Services
Web ---------> API over HTTP
SOAP --------> standalone CoreWCF service
```

## Projects

| Project                   | Responsibility                                                                                                     |
| ------------------------- | ------------------------------------------------------------------------------------------------------------------ |
| `Intertwine.API`          | REST controllers, dependency injection, JWT authentication, auth rate limiting, OpenAPI, and exception middleware. |
| `Intertwine.Services`     | Application rules, DTOs, JWT/refresh-token logic, services, and repository contracts.                              |
| `Intertwine.Domain`       | Entities, enums, abstractions, and domain interfaces.                                                              |
| `Intertwine.Repositories` | EF Core context, SQL Server mappings/migrations, repositories, unit of work, and Redis implementations.            |
| `Intertwine.Identity`     | ASP.NET Core Identity user model.                                                                                  |
| `Intertwine.Web`          | Vue 3 SPA using TypeScript, Vue Router, Pinia, Axios, and Vite.                                                    |
| `Intertwine.Worker`       | Hosted background process that creates Daily Question assignments in SQL Server.                                   |
| `Intertwine.SOAP`         | Standalone authenticated CoreWCF SOAP 1.1 payment-provider demonstration.                                          |
| `Intertwine.UnitTests`    | xUnit and Moq tests for API, services, repositories, caching, authentication, wallet, and worker behavior.         |

The solution entry point is `Intertwine.slnx`. The Worker and Web projects belong to the same solution and can still be deployed independently.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0), not only the runtime
- Node.js 24 and npm 11 recommended for the checked-in lockfile
- A SQL Server-compatible database
- Redis at `localhost:6379`, or another configured endpoint
- Git

### SQL Server by operating system

- **Windows:** the checked-in development connection uses SQL Server LocalDB.
- **macOS/Linux:** LocalDB is unavailable. Use SQL Server in a container, a remote SQL Server instance, or another reachable SQL Server-compatible environment. Override both the API and Worker connection strings.

Intertwine uses SQL Server-specific behavior, including a database trigger, so SQLite is not supported for normal application use.

## First-time setup

Run these commands from the repository root.

### 1. Restore dependencies

```bash
dotnet tool restore
dotnet restore Intertwine.slnx
npm ci --prefix Intertwine.Web
```

The local tool manifest installs the matching EF Core CLI. `npm ci` installs the exact frontend dependency resolution from `package-lock.json`.

### 2. Trust the development certificate

```bash
dotnet dev-certs https --trust
```

### 3. Configure SQL Server

The API and Worker must use the **same database**. Both projects have a committed `UserSecretsId`; secret values remain local to each computer and are not copied by Git.

Example for SQL Server on port 1433:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Server=localhost,1433;Database=Intertwine;User Id=sa;Password=<password>;TrustServerCertificate=True;MultipleActiveResultSets=true" \
  --project Intertwine.API

dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Server=localhost,1433;Database=Intertwine;User Id=sa;Password=<password>;TrustServerCertificate=True;MultipleActiveResultSets=true" \
  --project Intertwine.Worker
```

Windows developers can retain the LocalDB value in `appsettings.json` when LocalDB is installed.

### 4. Configure and verify Redis

The default is `localhost:6379,abortConnect=false`. Override it when Redis runs elsewhere:

```bash
dotnet user-secrets set "ConnectionStrings:RedisConnection" \
  "<redis-host>:6379,abortConnect=false" \
  --project Intertwine.API
```

If `redis-cli` is installed:

```bash
redis-cli ping
```

The expected response is `PONG`.

### 5. Apply migrations and seed data

```bash
dotnet ef database update \
  --project Intertwine.Repositories/Intertwine.Repositories.csproj \
  --startup-project Intertwine.API/Intertwine.API.csproj \
  --context IntertwineDbContext
```

Migrations create the schema, indexes, constraints, refresh-token storage, the UserAnswer protection trigger, and the initial catalog. The catalog seed contains 15 categories, 55 questions, their answer choices, and category links.

### 6. Check frontend configuration

The checked-in `.env` and `.env.example` use the Vite proxy:

```dotenv
VITE_API_URL=
API_PROXY_TARGET=https://localhost:7279
```

Values prefixed with `VITE_` are visible to browser code. Never put passwords, signing keys, or database credentials there.

## Run the application

Use separate terminals from the repository root.

### REST API

```bash
dotnet run --project Intertwine.API --launch-profile https
```

- HTTPS API: `https://localhost:7279`
- HTTP redirect endpoint: `http://localhost:5288`
- Development OpenAPI document: `https://localhost:7279/openapi/v1.json`

The project exposes OpenAPI JSON but does not currently include Swagger UI.

### Daily Question Worker

```bash
dotnet run --project Intertwine.Worker
```

The Worker creates assignments immediately, waits 24 hours after success, and retries after five minutes on failure. Its default range covers yesterday through seven days ahead relative to UTC, supporting active dates from UTC-12 through UTC+14. It uses SQL Server and does not require Redis.

See [Intertwine.Worker/README.md](Intertwine.Worker/README.md).

### Vue frontend

```bash
cd Intertwine.Web
npm run dev
```

Open the Vite URL, normally `http://127.0.0.1:5173`. The development proxy sends `/api` to the API HTTPS profile.

### Optional SOAP demonstration

```bash
dotnet run --project Intertwine.SOAP --launch-profile https
```

WSDL: `https://localhost:7019/PaymentGateway.svc?wsdl`

The SOAP provider is standalone and is not called by the REST wallet flow. See [Intertwine.SOAP/README.md](Intertwine.SOAP/README.md).

## Configuration

.NET loads `appsettings.json`, environment-specific settings, user secrets in Development, environment variables, and command-line values. Later sources override earlier ones.

| Key                                    | Used by       | Purpose                                                    |
| -------------------------------------- | ------------- | ---------------------------------------------------------- |
| `ConnectionStrings:DefaultConnection`  | API, Worker   | Shared SQL Server database.                                |
| `ConnectionStrings:RedisConnection`    | API           | Daily cache and answer idempotency.                        |
| `JwtSettings:Key`                      | API           | JWT signing key. Override outside local development.       |
| `JwtSettings:Issuer` / `Audience`      | API           | JWT validation.                                            |
| `JwtSettings:AccessTokenExpiryMinutes` | API           | Access-token lifetime; currently 60 minutes.               |
| `JwtSettings:RefreshTokenExpiryDays`   | API           | Sliding refresh lifetime; currently two days.              |
| `DailyQuestionWorker:*`                | Worker        | Assignment range and retry intervals.                      |
| `SoapAuthentication:ApiKey`            | SOAP          | SOAP API-key authentication.                               |
| `API_PROXY_TARGET`                     | Vite          | Development proxy destination.                             |
| `VITE_API_URL`                         | Browser build | Optional production API origin; public configuration only. |

Environment variables use double underscores, such as `ConnectionStrings__DefaultConnection`. Do not commit production secrets. A `UserSecretsId` is safe to commit because it is an identifier, not a credential.

## Important application behavior

### Authentication

- Registration creates an Identity account, `UserProfile`, and zero-balance `UserWallet`.
- Login returns a JWT and sets a Secure, HttpOnly refresh-token cookie.
- SQL Server stores only the refresh token's SHA-256 hash.
- Refresh rotates the token; logout revokes it and expires the cookie.
- Pinia stores the access token and user ID in `sessionStorage`. JavaScript never reads the refresh token.
- Axios adds the bearer token, shares one refresh across concurrent `401` responses, and retries eligible requests once.
- Login and registration are rate limited to five attempts per minute.

### Questions and answers

- `UserAnswers` stores the current selected answer, not answer history.
- The client supplies `localDate=YYYY-MM-DD` for daily rules.
- The first Daily Question answer consumes that date's Daily Question action; later changes on that date are rejected.
- New and changed non-daily answers each consume an action.
- Users receive two free non-daily actions per local date.
- Additional answers require explicit consent to spend 10 Sparks.
- Answer, activity, optional wallet debit, and ledger entry are committed in one EF Core unit of work.
- A SQL Server trigger provides a final guard against multiple current answers for one user/question.

### Redis

1. **Daily Question cache:** cache-aside data keyed by local date with a 24-hour TTL. Failures fall back to SQL Server.
2. **Answer idempotency:** user-scoped `Idempotency-Key` records prevent duplicate processing. In-progress claims expire after two minutes; completed responses remain for 24 hours. This path fails closed if Redis is unavailable.

Redis and the SQL transaction are separate systems, so this is not durable exactly-once execution across a process crash.

### Wallet and payments

- Wallet balances use integer Sparks.
- Top-up records a completed payment, credits the wallet, and appends a ledger entry.
- Wallet row versions provide optimistic concurrency protection.
- The REST provider is `IntertwineDemo`; no real money is charged.
- SOAP demonstrates API-key authentication, faults, and in-memory provider idempotency but is not integrated with REST top-up.

## Main API routes

| Area           | Routes                                                                          |
| -------------- | ------------------------------------------------------------------------------- |
| Authentication | `POST /api/Auth/register`, `/login`, `/refresh`, `/logout`                      |
| Questions      | `GET /api/questions`, `GET /api/questions/{id}`, `GET /api/questions/daily`     |
| Submit answer  | `POST /api/questions/{id}/answer?localDate=YYYY-MM-DD`                          |
| User state     | `GET /api/user-answers/me`, `GET /api/daily-activity/me`                        |
| Profile        | `POST`, `GET`, `PUT /api/UserProfile/me`; `POST /api/UserProfile/me/deactivate` |
| Wallet         | `GET /api/wallet`, `POST /api/wallet/top-up`, `GET /api/wallet/payments`        |
| Reference data | `GET /api/currencies`, `GET /api/credit-packages`                               |

`GET /api/questions/daily` and logout are anonymous at the controller boundary. Other application routes require JWT authorization. Logout still validates and revokes a refresh cookie when present.

Postman collections are documented in [Postman/README.md](Postman/README.md).

## Database migrations

Create a migration from the repository root:

```bash
dotnet ef migrations add <MigrationName> \
  --project Intertwine.Repositories/Intertwine.Repositories.csproj \
  --startup-project Intertwine.API/Intertwine.API.csproj \
  --context IntertwineDbContext
```

Review the migration and snapshot before committing. A data-only migration may intentionally leave the model snapshot unchanged. Do not manually paste the UserAnswer trigger into a database; `AddUserAnswerUniquenessTrigger` installs it.

## Testing and formatting

### Backend

```bash
dotnet build Intertwine.slnx --no-restore
dotnet test Intertwine.UnitTests/Intertwine.UnitTests.csproj --no-restore
```

### Frontend

```bash
cd Intertwine.Web
npm test
npm run format:check
npm run build
```

### Repository checks

```bash
git diff --check
git status --short
```

The repository uses `.editorconfig` and `.gitattributes`. Do not commit generated `bin/`, `obj/`, `node_modules/`, `dist/`, or `package.g.props` files. Commit `package-lock.json` when dependency resolution or the agreed npm version intentionally changes it.

## Troubleshooting

### LocalDB fails on macOS

LocalDB is Windows-only. Configure reachable SQL Server connection strings through user secrets for both API and Worker.

### The UI cannot reach the API

Confirm the API is listening on `https://localhost:7279`, the certificate is trusted, and `API_PROXY_TARGET` matches. Open the frontend through Vite during development.

### Redis times out

Verify Redis with `redis-cli ping`. Daily Question reads fall back to SQL, but answer submissions require Redis to acquire their idempotency claim.

### The UI shows “No Daily Question”

Apply migrations, confirm seeded questions exist, start the Worker, inspect its logs, and verify API and Worker use the same database.

### User secrets are absent after cloning

This is expected. The ID is cloned; secret values live outside the repository on each computer. Without them, .NET uses the remaining configuration sources.

### `package-lock.json` changes on another computer

Use the recommended Node/npm versions and `npm ci`. npm versions can normalize `dev`, `peer`, and `devOptional` metadata without changing package versions. Review the diff instead of manually editing it.

## Current boundaries

- Top-up is simulated; there is no real payment processor or webhook.
- SOAP is implemented as a standalone demonstration.
- SignalR, a message broker, microservices, and database sharding are not implemented.
- Daily assignment uses a separately runnable Worker, not a message queue.
- Access JWTs remain valid until expiry; logout revokes the refresh session.
- Redis improves caching and retry safety; SQL Server remains the source of truth.

For frontend screens and contracts, see [Intertwine.Web/README.md](Intertwine.Web/README.md).
