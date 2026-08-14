# Ostav.Api deployment

Build from the repository root so the backend can compile the shared platform-neutral Core sources:

```powershell
docker build -f Platform/Ostav.Api/Dockerfile -t ostav-api:dev .
```

Run a development container with PostgreSQL:

```powershell
docker run --rm -p 8080:8080 `
  -e ASPNETCORE_ENVIRONMENT=Development `
  -e PORT=8080 `
  -e Ostav__ApiKey=replace-with-development-key `
  -e Ostav__Persistence__Mode=PostgreSQL `
  -e OSTAV_POSTGRES_CONNECTION='Host=...;Database=...;Username=...;Password=...;SSL Mode=Require' `
  ostav-api:dev
```

Required variables are `Ostav__ApiKey`, `Ostav__Persistence__Mode`, and, for PostgreSQL mode, `OSTAV_POSTGRES_CONNECTION`. `ASPNETCORE_ENVIRONMENT` selects the environment name and `PORT` selects the HTTP listener port. The health endpoint is `GET /health/live`; readiness is `GET /health/ready`.

Supabase Session Pooler is supported as standard PostgreSQL infrastructure. Preserve the provider's SSL settings in the externally supplied connection string. Never commit or log a database password, connection string, or API key.
