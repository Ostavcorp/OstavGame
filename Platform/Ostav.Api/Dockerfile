FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Platform/Ostav.Api/Ostav.Api.csproj Platform/Ostav.Api/
COPY Platform/Ostav.Host/Ostav.Host.csproj Platform/Ostav.Host/
COPY Platform/Ostav.Persistence/Ostav.Persistence.csproj Platform/Ostav.Persistence/
COPY GameClient/Assets/Ostav/Core/Runtime/ GameClient/Assets/Ostav/Core/Runtime/
RUN dotnet restore Platform/Ostav.Api/Ostav.Api.csproj

COPY Platform/Ostav.Api/ Platform/Ostav.Api/
COPY Platform/Ostav.Host/ Platform/Ostav.Host/
COPY Platform/Ostav.Persistence/ Platform/Ostav.Persistence/
RUN dotnet publish Platform/Ostav.Api/Ostav.Api.csproj --configuration Release --output /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV PORT=8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
USER root
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish/ ./
USER app
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 CMD curl --fail --silent http://127.0.0.1:${PORT}/health/live || exit 1
ENTRYPOINT ["dotnet", "Ostav.Api.dll"]
