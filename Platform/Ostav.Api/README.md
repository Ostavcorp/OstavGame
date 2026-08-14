# Ostav.Api

Requires the installed .NET 10 SDK.

Set a development API key, then run:

```powershell
$env:Ostav__ApiKey = "replace-with-a-development-secret"
dotnet run --project Platform/Ostav.Api/Ostav.Api.csproj
```

- Health: `http://localhost:5000/health/live` and `/health/ready`
- Execute: `http://localhost:5000/api/v1/execute`

```powershell
$headers = @{ "X-Ostav-Api-Key" = $env:Ostav__ApiKey }
$body = '{"apiVersion":"1.0","requestId":"demo-1","correlationId":"demo-c","targetCapabilityId":"system","intentType":"system.ping","locale":"en"}'
Invoke-RestMethod http://localhost:5000/api/v1/execute -Method Post -Headers $headers -ContentType application/json -Body $body
```

The development API key is not production authentication. OAuth/OIDC and user authentication are not implemented.

Persistence defaults to process-local memory, which is suitable for tests and disposable development runs:

```powershell
$env:Ostav__Persistence__Mode = "InMemory"
```

For durable local development, select SQLite and provide an external database path:

```powershell
$env:Ostav__Persistence__Mode = "SQLite"
$env:Ostav__Persistence__DatabasePath = "C:\path\outside-source\ostav-development.db"
```

For PostgreSQL, select the PostgreSQL mode and supply the connection string only through configuration or the environment:

```powershell
$env:Ostav__Persistence__Mode = "PostgreSQL"
$env:OSTAV_POSTGRES_CONNECTION = "Host=...;Database=...;Username=...;Password=...;SSL Mode=Require"
```

Supabase Session Pooler is supported as standard PostgreSQL infrastructure; no Supabase SDK is used. Preserve the SSL settings required by the provider. Never commit or log the database password or connection string.

PostgreSQL integration tests are opt-in and use `OSTAV_POSTGRES_TEST_CONNECTION`. The supplied test account must be isolated from production data and able to create and remove a temporary test schema.

The API key authenticates a development service request only. The development identity bridge maps authenticated requests to a persisted Ostav identity; it is not production user authentication. Production OAuth/OIDC is not implemented.
