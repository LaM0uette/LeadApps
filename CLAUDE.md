# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

All commands run from the repository root. There is **no test project** in this solution.

```bash
dotnet build LeadApps.sln
dotnet run --project TopDeck/TopDeck            # Blazor app  -> https://localhost:7184
dotnet run --project TopDeck/TopDeck.Api        # REST API    -> https://localhost:7095 (redirects / to /swagger)
```

The app and the API are two independent processes; the app calls the API over HTTP, so both must run for a full local session.

### EF Core migrations (TopDeck.Api only)

```bash
dotnet ef migrations add <Name> --project TopDeck/TopDeck.Api
dotnet ef database update --project TopDeck/TopDeck.Api
```

Migrations are applied automatically at API startup (`db.Database.Migrate()` in `TopDeck.Api/Program.cs`), so `database update` is rarely needed — just run the API.

Local DB is Postgres, connection string in `TopDeck.Api/appsettings.Development.json` (`topdeck-dev` on `localhost:5432`). Schema is `data`. In Preprod/Prod the connection string **must** come from the `ConnectionStrings__Default` environment variable; the API throws at startup if it's missing.

### Deployment

Two manual GitHub Actions (`.github/workflows/deploy-app.yml`, `deploy.yml`), each with a `prod`/`preprod` choice. They build the Dockerfile, push to GHCR, then SSH to the VPS and `docker compose up`. Dockerfile build contexts are the **repo root**, not the project folder.

## Architecture

### Solution layout

Six projects, all under `TopDeck/`, targeting net9.0:

| Project | Role |
| --- | --- |
| `TopDeck` | ASP.NET Core host — Blazor Web App server, Auth0 login endpoints, static assets, localization JSON |
| `TopDeck.Client` | Blazor WebAssembly — routable pages and `MainLayout` |
| `TopDeck.Shared` | Razor class library — reusable components, API services, and the `Modules/` feature libraries |
| `TopDeck.Contracts` | DTOs shared by the API and the front-end (no dependencies) |
| `TopDeck.Domain` | Domain records used by the front-end (no dependencies) |
| `TopDeck.Api` | Minimal-API REST service over EF Core / Postgres |

Reference chain: `TopDeck` → `TopDeck.Client` → `TopDeck.Shared` → `TopDeck.Contracts` + `TopDeck.Domain`. `TopDeck.Api` references only `TopDeck.Contracts`.

### Two backends, not one

The front-end talks to **two separate APIs**, and both Program.cs files resolve their base URLs from a `switch` on the environment name (`Development` / `Preproduction` / `Production`, throwing on anything else):

- **LeaderSheep API** = `TopDeck.Api` **in this repo** (`https://localhost:7095`). Owns users, decks, deck suggestions, votes, tags. Consumed through the `TopDeck.Shared/Services/Api/*` services (`IUserService`, `IDeckItemService`, `IDeckDetailsService`, `IVoteService`, `ITagService`).
- **TopDeck card API** = an **external service not in this repo** (`https://localhost:7057` locally, `api.proflam0uette.fr` in prod). Serves Pokémon TCG Pocket card data under `/cards/*`. Consumed only through `ITCGPCardRequester`.

Card-related bugs are usually in `TCGPCardRequester`, deck/user bugs in `TopDeck.Api`.

### Render mode and the double DI registration

The app uses global `InteractiveWebAssembly` (`Components/App.razor`), with the auth state serialized from server to WASM. This means **every service must be registered twice** — once in `TopDeck/Program.cs` (server prerender pass) and once in `TopDeck.Client/Program.cs` (WASM runtime). Forgetting the second registration produces an error only after the client takes over.

The two registrations are deliberately *not* identical: the server registers `FakeAuthUserRequester` (and `TopDeck/FakeServices/Api/*` exist for the same reason) so prerendering doesn't fan out into API calls it can't authenticate.

### Front-end page pattern: Presenter

Pages are split into three files and use inheritance, **not** `partial class`:

- `XxxPage.razor` — markup, starts with `@page`, `@inherits XxxPagePresenter`, `@attribute [AllowAnonymous]` where relevant
- `XxxPage.razor.cs` — a `XxxPagePresenter : PresenterBase` class holding all state and logic
- `XxxPage.razor.css` — scoped styles

`PresenterBase` (`TopDeck.Shared/Components/Presenter/`) injects `JS`, `Localizer`, `NavigationManager`, `UIStore`, exposes `IsMobile` (< 768px) via a JS resize handler, and implements `IAsyncDisposable`. Override `DisposeAsync` with `base.DisposeAsync()` when a presenter adds its own `DotNetObjectReference`. `MainLayout` follows the same shape with `MainLayoutBase : LayoutComponentBase`.

Filters and paging are bound to the query string with `[SupplyParameterFromQuery]` and re-read in `OnParametersSetAsync`, so navigation is the state-change mechanism.

### State: the BFlux store

`TopDeck.Shared/Modules/UniFlux/` is a hand-rolled Redux-like store (namespace `BFlux`) — do not confuse the folder name with the namespace. `UIStore` is a singleton `Store` subclass registering its states in the constructor. Adding state means: a `record XxxState(...) : ImmutableState`, a `record SetXxxAction(...) : ImmutableAction<XxxState>` overriding `Reduce`, and a `States.Add(new XxxState(...))` line in `UIStore`. Read with `GetState<T>()`, write with `Dispatch(action)`, observe with `Subscribe<T>()` (dispose the returned `IDisposable`). Transient one-shot events use the parallel `Emit`/`Listen` signal API.

### API request flow

`Endpoints → Service → Repository → ApplicationDbContext`. Endpoints are static classes with a `MapXxxEndpoints` extension using `MapGroup`, wired in `TopDeck.Api/Program.cs`; handlers are `private static` methods taking `[FromServices]` dependencies and returning `IResult`. The DbContext is pooled with `QueryTrackingBehavior.NoTracking` globally, and `SaveChanges` auto-stamps any `CreatedAt`/`UpdatedAt` property by convention.

### Three parallel model layers

The same concept exists three times, and mapping is manual extension methods — there is no AutoMapper:

- `TopDeck.Api/Entities/*` — EF entities (mutable classes)
- `TopDeck.Contracts/DTO/*` — wire records, `Input`/`Output` suffixed, `DTO` in all caps
- `TopDeck.Domain/Models/*` — front-end records

`TopDeck.Api/Mappings/*` converts entity ↔ DTO; `TopDeck.Shared/Mappings/*` converts DTO → domain. A change to a shape usually needs edits in all three plus both mappers.

### Authentication

Auth0 OIDC on the server host only. `/Account/Login` and `/Account/Logout` (`TopDeck/Endpoints/AuthEndpoints.cs`) issue the challenge; the optional `provider` query parameter selects an Auth0 connection. `Auth0SubHelper.TryParse` splits the Auth0 `sub` claim into `provider` + `authId` — that pair, not the Auth0 id, is the unique key on `User`.

First-login flow: `OnTokenValidated` POSTs the user to `/users` with the placeholder username `__unknown__` (the API never overwrites an existing user); `MainLayoutBase.OnInitializedAsync` detects that placeholder and redirects to `/profile/pseudo`.

### Localization

`JsonLocalizer` (namespace `Localizer`) fetches `wwwroot/locales/{culture}.json` — a flat key→string dictionary — from the host, and is initialized in `TopDeck.Client/Program.cs` before `RunAsync`. Call sites always pass a fallback: `Localizer.Localize("page.x.y.text", "Fallback")`. Supported cultures are `en` and `fr`; `?lng=xx` overrides the browser culture. Adding a string means editing **both** `en.json` and `fr.json` under `TopDeck/wwwroot/locales/`.

### CSS and JS interop

Global stylesheets live in `TopDeck/wwwroot/css/` (`reset`, `fonts`, `default`, `app`); everything else is scoped `.razor.css`. Theming is a `data-theme` attribute driven by `theme.js`, so colors go through CSS custom properties, never hard-coded.

JS helpers live in `TopDeck.Shared/wwwroot/*.js` (`infiniteScroll`, `windowSize`, `scrollTo`, `back`, `theme`, `svgLoader`), are loaded as `_content/TopDeck.Shared/<name>.js` from `App.razor`, and expose globals (`TopDeck.*`, `TopDeckTheme.*`, `registerResizeHandler`). Any new script must be added to the `App.razor` script list. Guard interop that may run during prerender — the existing code checks `JS is IJSInProcessRuntime` or `JS.GetType().Name != "UnsupportedJavaScriptRuntime"`.

## Gotchas

- **Namespaces are feature-scoped, not folder-derived.** `TopDeck.Shared/Modules/UniFlux/` → `BFlux`, `Modules/Localizer/` → `Localizer`, `Modules/Requesters/AuthUser/` → `Requesters.AuthUser`, and every `Services/Api/<Feature>/` file → `TopDeck.Shared.Services`. Match the neighbouring file, don't infer from the path.
- `TopDeck.Domain.Models.Tag` collides with the `Tag.razor` component and with `TopDeck.Api.Entities.Tag`; existing code aliases it (`using DomainTag = TopDeck.Domain.Models.Tag;` or `@using Tag = TopDeck.Domain.Models.Tag`).
- The card-set ordering table (`_collectionOrder`, `A1`…`P-B`) and `_pokemonTypeOrder` are **duplicated** in `DeckDetailsPage.razor.cs` and `DeckDetailsEditPage.razor.cs`. New sets must be added to both.
- The API's CORS policy hard-codes `https://localhost:7184`; running the app on another port breaks browser calls to the API.
