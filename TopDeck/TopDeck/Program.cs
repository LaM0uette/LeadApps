using System.Globalization;
using System.Security.Claims;
using Auth0.AspNetCore.Authentication;
using Helpers.Auth0;
using Localizer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Requesters.AuthUser;
using TCGPCardRequester;
using TopDeck.Components;
using TopDeck.Contracts.DTO;
using TopDeck.Endpoints;
using TopDeck.FakeServices;
using TopDeck.Shared.Services;
using TopDeck.Shared.UIStore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Détermination des URLs selon l'environnement
string topdeckApiUrl = builder.Environment.EnvironmentName switch
{
    "Development"   => "https://localhost:7057",
    "Preproduction" => "https://api.preprod.proflam0uette.fr",
    "Production"    => "https://api.proflam0uette.fr",
    _ => throw new Exception($"Environnement inconnu : {builder.Environment.EnvironmentName}")
};

string leadersheepApiUrl = builder.Environment.EnvironmentName switch
{
    "Development"   => "https://localhost:7095",
    "Preproduction" => "https://api.preprod.tehleadersheep.com",
    "Production"    => "https://api.tehleadersheep.com",
    _ => throw new Exception($"Environnement inconnu : {builder.Environment.EnvironmentName}")
};

// HttpClient générique
builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri(leadersheepApiUrl);
});


builder.Services
    .AddAuth0WebAppAuthentication(options => {
        options.Domain = builder.Configuration["Auth0:Domain"] ?? throw new InvalidOperationException("❌ Auth0:Domain missing");
        options.ClientId = builder.Configuration["Auth0:ClientId"] ?? throw new InvalidOperationException("❌ Auth0:ClientId missing");
        options.OpenIdConnectEvents = new OpenIdConnectEvents
        {
            OnTokenValidated = async context =>
            {
                ClaimsPrincipal? user = context.Principal;
                string? sub = user?.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (!Auth0SubHelper.TryParse(sub, out string provider, out string authId)) 
                    return;

                string? email =
                    user?.FindFirstValue(ClaimTypes.Email) ??
                    user?.Claims.FirstOrDefault(c => c.Type == "email")?.Value ??
                    (user?.FindFirstValue(ClaimTypes.Name)?.Contains('@') == true ? user.FindFirstValue(ClaimTypes.Name) : null);

                string fullName =
                    user?.FindFirstValue(ClaimTypes.Name) ??
                    $"{user?.FindFirstValue(ClaimTypes.GivenName)} {user?.FindFirstValue(ClaimTypes.Surname)}".Trim();

                string? nickname = user?.Claims.FirstOrDefault(c => c.Type == "nickname")?.Value;

                string userName = !string.IsNullOrWhiteSpace(nickname)
                    ? nickname
                    : !string.IsNullOrWhiteSpace(fullName) ? fullName : email ?? "unknown";

                IHttpClientFactory factory = context.HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>();
                HttpClient http = factory.CreateClient("Api");

                UserInputDTO dto = new(provider, authId, userName);
                await http.PostAsJsonAsync("users", dto, context.HttpContext.RequestAborted);
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();

builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

// Services
builder.Services.AddSingleton<UIStore>();
builder.Services.AddScoped<ILocalizer, JsonLocalizer>();


builder.Services.AddScoped<IAuthUserRequester, FakeAuthUserRequester>();



// TCGPCardRequester (TopDeck API)
builder.Services.AddHttpClient<ITCGPCardRequester, TCGPCardRequester.TCGPCardRequester>(client =>
{
    client.BaseAddress = new Uri(topdeckApiUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// LeaderSheep API services
builder.Services.AddHttpClient<IUserService, UserService>(client =>
{
    client.BaseAddress = new Uri(leadersheepApiUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<IDeckDetailsService, DeckDetailsService>(client =>
{
    client.BaseAddress = new Uri(leadersheepApiUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<IDeckItemService, DeckItemService>(client =>
{
    client.BaseAddress = new Uri(leadersheepApiUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<IVoteService, VoteService>(client =>
{
    client.BaseAddress = new Uri(leadersheepApiUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<ITagService, TagService>(client =>
{
    client.BaseAddress = new Uri(leadersheepApiUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});




string[] supportedCultures = ["en", "fr"];
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    List<CultureInfo> cultures = supportedCultures.Select(c => new CultureInfo(c)).ToList();
    options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("en");
    options.SupportedCultures = cultures;
    options.SupportedUICultures = cultures;
});

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

// Dites à l'app de respecter les headers envoyés par le proxy
ForwardedHeadersOptions forwardedHeaderOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
// important si ton proxy et ton app sont dans le même réseau docker
forwardedHeaderOptions.KnownNetworks.Clear();
forwardedHeaderOptions.KnownProxies.Clear();

app.UseForwardedHeaders();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();
app.UseRequestLocalization();

app.MapAuthEndpoints();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(TopDeck.Client._Imports).Assembly);

app.Run();