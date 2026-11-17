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

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "PoliMarket.Api.dll"]
