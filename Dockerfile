FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file and restore (ajustar caminho)
COPY PackingService.Api/PackingService.Api.csproj ./PackingService.Api/
RUN dotnet restore PackingService.Api/PackingService.Api.csproj

# Copy all source code
COPY . .

# Build and publish (ajustar caminho)
WORKDIR /src/PackingService.Api
RUN dotnet publish PackingService.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PackingService.Api.dll"]