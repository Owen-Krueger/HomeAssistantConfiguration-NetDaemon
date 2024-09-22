FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Restore
WORKDIR /src
COPY ["src/NetDaemon.csproj", "./"]
RUN dotnet restore "./NetDaemon.csproj"

# Build
RUN dotnet build "./NetDaemon.csproj" -c Release -o /app/build

# Publish
RUN dotnet publish "./NetDaemon.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NetDaemon.dll"]
