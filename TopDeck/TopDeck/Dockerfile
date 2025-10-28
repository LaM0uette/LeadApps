# Build étape
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY LeadApps.sln ./
COPY TopDeck/TopDeck.Client/TopDeck.Client.csproj TopDeck/TopDeck.Client/
COPY TopDeck/TopDeck.Shared/TopDeck.Shared.csproj TopDeck/TopDeck.Shared/
COPY TopDeck/TopDeck.Contracts/TopDeck.Contracts.csproj TopDeck/TopDeck.Contracts/
COPY TopDeck/TopDeck.Domain/TopDeck.Domain.csproj TopDeck/TopDeck.Domain/
COPY TopDeck/TopDeck/TopDeck.csproj TopDeck/TopDeck/
RUN dotnet restore

COPY . .
WORKDIR /src/TopDeck/TopDeck.Client
RUN dotnet publish -c Release -o /app/publish

# Serveur Nginx
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

# Efface la page d'accueil par défaut
RUN rm -rf ./*

# Copie le contenu du wwwroot publié dans nginx
COPY --from=build /app/publish/wwwroot .

# Copie une conf nginx custom si tu veux gérer SPA / fallback
# COPY nginx.conf /etc/nginx/conf.d/default.conf
