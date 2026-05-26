# Build stage — context must be the solution root (docker-compose sets this)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files first to leverage Docker layer caching for restore
COPY API/API.csproj                        API/
COPY Application/Application.csproj        Application/
COPY Domain/Domain.csproj                  Domain/
COPY Infrastructure/Infrastructure.csproj  Infrastructure/

RUN dotnet restore API/API.csproj

# Copy full source and publish
COPY API/            API/
COPY Application/    Application/
COPY Domain/         Domain/
COPY Infrastructure/ Infrastructure/

RUN dotnet publish API/API.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "API.dll"]
