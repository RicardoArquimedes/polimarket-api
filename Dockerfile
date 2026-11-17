# ---- Build ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY PoliMarket.Domain/PoliMarket.Domain.csproj PoliMarket.Domain/
COPY PoliMarket.Components/PoliMarket.Components.csproj PoliMarket.Components/
COPY PoliMarket.Api/PoliMarket.Api.csproj PoliMarket.Api/
RUN dotnet restore PoliMarket.Api/PoliMarket.Api.csproj

COPY . .
RUN dotnet publish PoliMarket.Api/PoliMarket.Api.csproj -c Release -o /app/publish --no-restore

# ---- Runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

# Render inyecta PORT en tiempo de ejecucion (por defecto 10000); en local, sin -e PORT, usa 8080.
ENTRYPOINT ["/bin/sh", "-c", "exec dotnet PoliMarket.Api.dll --urls http://0.0.0.0:${PORT:-8080}"]
