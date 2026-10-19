FROM mcr.microsoft.com/dotnet/sdk:10.0.402-noble AS restore
WORKDIR /source

COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/backend/Atmos.Domain/Atmos.Domain.csproj src/backend/Atmos.Domain/
COPY src/backend/Atmos.Application/Atmos.Application.csproj src/backend/Atmos.Application/
COPY src/backend/Atmos.Infrastructure/Atmos.Infrastructure.csproj src/backend/Atmos.Infrastructure/
COPY src/backend/Atmos.Api/Atmos.Api.csproj src/backend/Atmos.Api/
RUN dotnet restore src/backend/Atmos.Api/Atmos.Api.csproj

FROM restore AS publish
COPY src/backend src/backend
RUN dotnet publish src/backend/Atmos.Api/Atmos.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0.10-noble AS final
USER root
RUN apt-get update \
    && apt-get install --yes --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=publish --chown=$APP_UID:$APP_UID /app/publish .
USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_EnableDiagnostics=0
EXPOSE 8080
HEALTHCHECK --interval=15s --timeout=3s --start-period=10s --retries=3 \
    CMD curl --fail --silent http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "Atmos.Api.dll"]
