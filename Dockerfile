# syntax=docker/dockerfile:1.7
# Multi-stage build for OpenMES.Web. Run from the repo root:
#   docker build -t openmes/web .
# or via compose:
#   docker compose --profile full up -d --build

# ---- Stage 1: restore + publish ---------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only the csproj files first so `dotnet restore` is cached on the layer
# above the source tree.
COPY src/OpenMES.Domain/OpenMES.Domain.csproj                       src/OpenMES.Domain/
COPY src/OpenMES.PluginAbstractions/OpenMES.PluginAbstractions.csproj src/OpenMES.PluginAbstractions/
COPY src/OpenMES.Application/OpenMES.Application.csproj             src/OpenMES.Application/
COPY src/OpenMES.Infrastructure/OpenMES.Infrastructure.csproj       src/OpenMES.Infrastructure/
COPY src/OpenMES.Web/OpenMES.Web.csproj                             src/OpenMES.Web/
COPY src/OpenMES.Worker/OpenMES.Worker.csproj                       src/OpenMES.Worker/

RUN dotnet restore src/OpenMES.Web/OpenMES.Web.csproj

# Now bring in the source and publish.
COPY src/ src/
RUN dotnet publish src/OpenMES.Web/OpenMES.Web.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ---- Stage 2: runtime --------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_EnableDiagnostics=0

# Connector inputs are expected at /app/connectors/. Either bind-mount your
# CSV / docs folder there, or override OpenMes__Connectors__* env vars.
ENV OpenMes__Connectors__CsvJobsPath= \
    OpenMes__Connectors__FileSystemDocumentsRoot=

EXPOSE 8080
COPY --from=build /app/publish ./

# Drop privileges for runtime — the aspnet base image already ships a
# non-root `app` user.
USER app

ENTRYPOINT ["dotnet", "OpenMES.Web.dll"]
