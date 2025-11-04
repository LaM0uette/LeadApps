FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY TopDeck/TopDeck/TopDeck.csproj TopDeck/TopDeck/
COPY TopDeck/TopDeck.Client/TopDeck.Client.csproj TopDeck/TopDeck.Client/
COPY TopDeck/TopDeck.Shared/TopDeck.Shared.csproj TopDeck/TopDeck.Shared/
COPY TopDeck/TopDeck.Contracts/TopDeck.Contracts.csproj TopDeck/TopDeck.Contracts/
COPY TopDeck/TopDeck.Domain/TopDeck.Domain.csproj TopDeck/TopDeck.Domain/

RUN dotnet restore TopDeck/TopDeck/TopDeck.csproj

COPY . .
RUN dotnet publish TopDeck/TopDeck/TopDeck.csproj -c Release -o /app/web --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app
COPY --from=build /app/web .
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "TopDeck.dll"]
