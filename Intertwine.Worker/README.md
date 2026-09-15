# Intertwine Daily Question Worker

The Worker immediately ensures Daily Question assignments exist when it starts. After a successful run it waits 24 hours; after a failure it waits five minutes before retrying.

```powershell
dotnet run --project Intertwine.Worker/Intertwine.Worker.csproj
```

The default date window starts one day before the current UTC date and ends seven days after it. This covers the active local dates at UTC-12 and UTC+14 while retaining a future assignment buffer.

The Worker depends on SQL Server for assignments. It does not depend on Redis because the assignment table is the source of truth and the API does not cache missing Daily Questions. Redis remains responsible for caching successful Daily Question reads.

Configuration is under `DailyQuestionWorker` in `appsettings.json`:

- `PastDaysToCover`: dates before UTC today.
- `FutureDaysToCover`: dates after UTC today.
- `SuccessIntervalHours`: delay following a successful run.
- `FailureRetryMinutes`: delay following a failed run.
