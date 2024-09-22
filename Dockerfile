FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

WORKDIR /src
COPY *.csproj ./
RUN dotnet restore "src/NetDaemon.csproj"

COPY . ./
RUN dotnet publish -c Release -o out ./NetDaemon.csproj

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "NetDaemon.dll"]
