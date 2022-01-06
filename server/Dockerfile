# BUILD
FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine as build
WORKDIR /app
COPY . .
RUN dotnet restore AccTelemetryTracker.Api/AccTelemetryTracker.Api.csproj
RUN dotnet publish -o /app/publish AccTelemetryTracker.Api/AccTelemetryTracker.Api.csproj

# RUN
FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine as runtime
WORKDIR /app
COPY --from=build /app/publish /app

EXPOSE 80

ENTRYPOINT [ "dotnet", "/app/AccTelemetryTracker.Api.dll" ]
