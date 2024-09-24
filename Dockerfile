FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Restore
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/NetDaemon.csproj", "./"]
RUN dotnet restore "./NetDaemon.csproj"
COPY . .

# Build
WORKDIR '/src/'
RUN dotnet build "NetDaemon.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "NetDaemon.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NetDaemon.dll"]
