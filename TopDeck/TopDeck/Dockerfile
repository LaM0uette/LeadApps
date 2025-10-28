# Étape build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY LeadApps.sln ./
COPY TopDeck/TopDeck.Client/TopDeck.Client.csproj TopDeck/TopDeck.Client/
COPY TopDeck/TopDeck.Shared/TopDeck.Shared.csproj TopDeck/TopDeck.Shared/
COPY TopDeck/TopDeck.Contracts/TopDeck.Contracts.csproj TopDeck/TopDeck.Contracts/
COPY TopDeck/TopDeck.Domain/TopDeck.Domain.csproj TopDeck/TopDeck.Domain/
COPY TopDeck/TopDeck/TopDeck.csproj TopDeck/TopDeck/

RUN dotnet restore TopDeck/TopDeck.Client/TopDeck.Client.csproj

COPY . .
WORKDIR /src/TopDeck/TopDeck.Client
RUN dotnet publish -c Release -o /app/publish

# Étape runtime : on sert via nginx (plus performant pour du statique)
FROM nginx:alpine AS runtime
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
EXPOSE 80
