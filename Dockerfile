# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["Slottet/Slottet.csproj", "Slottet/"]
RUN dotnet restore "Slottet/Slottet.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/Slottet"
RUN dotnet build "Slottet.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Slottet.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Slottet.dll"]
