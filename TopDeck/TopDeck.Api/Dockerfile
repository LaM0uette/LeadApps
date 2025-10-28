# ----------- BUILD STAGE -----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copie solution + csproj
COPY LeadApps.sln ./
COPY TopDeck/TopDeck.Api/TopDeck.Api.csproj TopDeck/TopDeck.Api/
COPY TopDeck/TopDeck.Contracts/TopDeck.Contracts.csproj TopDeck/TopDeck.Contracts/
COPY TopDeck/TopDeck.Domain/TopDeck.Domain.csproj TopDeck/TopDeck.Domain/
COPY TopDeck/TopDeck.Shared/TopDeck.Shared.csproj TopDeck/TopDeck.Shared/

# Restore
RUN dotnet restore

# Copie tout et build
COPY . .
WORKDIR /src/TopDeck/TopDeck.Api
RUN dotnet publish -c Release -o /app/publish

# ----------- RUNTIME STAGE -----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "TopDeck.Api.dll"]
