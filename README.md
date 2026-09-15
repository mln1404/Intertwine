# Intertwine API

Intertwine is a layered ASP.NET Core API for question-based user matching. The solution separates API, Services, Domain, Repositories, and Identity concerns.

## Run locally

1. Start SQL Server and Redis. The development Redis connection is configured as `localhost:6379` in `Intertwine.API/appsettings.json`.
2. Apply the EF Core migrations before running the API.
3. Start `Intertwine.API`. The HTTP profile listens at `http://localhost:5288`.

Redis is used for answer-submission idempotency. It is an API dependency: answer submissions will fail rather than risk duplicate processing when Redis is unavailable.

## Daily Question caching

`GET /api/questions/daily?localDate=2026-09-15` is anonymous and uses a cache-aside Redis read. Its key contains the supplied local date, so users at UTC+14 and UTC-12 can receive and cache their respective Daily Questions independently. Each date-specific value expires after 24 hours.

On a cache miss, the API queries the Daily Question assignment together with the question's categories and active answers, maps it to the response DTO, and stores that result in Redis. A missing assignment returns `404 Not Found` and is not cached.

## Daily Question worker API

The separate worker solution lives beside this repository in `../Intertwine.Worker/Intertwine.Worker.slnx`. It references the shared projects in this repository. `DailyQuestionService` owns assignment selection, and `DailyQuestionRepository` owns database reads and inserts; their interfaces, unit tests, and EF migrations remain here.

See the [worker setup and demo instructions](../Intertwine.Worker/README.md) for UTC-12 scheduling, the anonymous manual endpoint, configuration, and rate limits.

## Postman answer-submission demo

Obtain a bearer token through the authentication endpoint, then send:

```http
POST http://localhost:5288/api/questions/{questionId}/answer?localDate=2026-09-15
Authorization: Bearer {access-token}
Idempotency-Key: 6d5b0d2b-6c56-4df5-a2d8-3b6cb24e6f25
Content-Type: application/json

{
  "answerId": 1
}
```

The idempotency key is scoped to the authenticated user. Repeating the exact request with the same key returns the original successful response without running the answer workflow again. Reusing that key for a different question, answer, or local date returns `409 Conflict`. A concurrently processing identical request also returns `409 Conflict`.

The service commits the answer and its daily-activity update in one database save. A new or changed non-daily answer consumes one of the two daily non-daily slots. A first Daily Question answer marks the daily action as used; later Daily Question actions on that local date are rejected.

## Verification

```powershell
dotnet build Intertwine.slnx
dotnet test Intertwine.UnitTests/Intertwine.UnitTests.csproj
dotnet test ../Intertwine.Worker/Intertwine.Worker.slnx
```

The unit-test project uses xUnit and Moq. It covers answer-submission rules, Daily Question caching, assignment selection, idempotent reruns, and deleted-date recovery. `Intertwine.Worker.Tests` exercises UTC-12 boundaries, startup and 24-hour scheduling, recovery after failures, overlapping calls, repository queries, date uniqueness, and the anonymous endpoint's per-IP rate limit. HTTP/repository tests use a temporary SQLite database and a mocked cache; they do not connect to or modify the configured SQL Server/Redis instances. SQL Server-specific duplicate-key exception handling still requires validation against SQL Server when deploying.
