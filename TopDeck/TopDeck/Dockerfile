# Étape build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY LeadApps.sln ./
COPY TopDeck/TopDeck/TopDeck.csproj TopDeck/TopDeck/
COPY TopDeck/TopDeck.Api/TopDeck.Api.csproj TopDeck/TopDeck.Api/
COPY TopDeck/TopDeck.Client/TopDeck.Client.csproj TopDeck/TopDeck.Client/
COPY TopDeck/TopDeck.Shared/TopDeck.Shared.csproj TopDeck/TopDeck.Shared/
COPY TopDeck/TopDeck.Contracts/TopDeck.Contracts.csproj TopDeck/TopDeck.Contracts/
COPY TopDeck/TopDeck.Domain/TopDeck.Domain.csproj TopDeck/TopDeck.Domain/
RUN dotnet restore

COPY . .
WORKDIR /src/TopDeck/TopDeck
RUN dotnet publish -c Release -o /app/publish

# Étape runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "TopDeck.dll"]
