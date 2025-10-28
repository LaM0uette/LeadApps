using Localizer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Requesters.AuthUser;
using TCGPCardRequester;
using TopDeck.Shared.Services;
using TopDeck.Shared.UIStore;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

// HttpClient par défaut (pour les assets Blazor)
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Détermination des URLs selon l'environnement
string topdeckApiUrl = builder.HostEnvironment.Environment switch
{
    "Development"   => "https://localhost:7057",
    "Preproduction" => "https://api.preprod.proflam0uette.fr",
    "Production"    => "https://api.proflam0uette.fr",
    _ => throw new Exception($"Environnement inconnu : {builder.HostEnvironment.Environment}")
};

string leadersheepApiUrl = builder.HostEnvironment.Environment switch
{
    "Development"   => "https://localhost:7095",
    "Preproduction" => "https://api.preprod.tehleadersheep.com",
    "Production"    => "https://api.tehleadersheep.com",
    _ => throw new Exception($"Environnement inconnu : {builder.HostEnvironment.Environment}")
};

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();
builder.Services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();

// Services
builder.Services.AddSingleton<UIStore>();
builder.Services.AddScoped<ILocalizer, JsonLocalizer>();

builder.Services.AddScoped<IAuthUserRequester, AuthUserRequester>();



// TCGPCardRequester (TopDeck API)
builder.Services.AddScoped<ITCGPCardRequester>(_ =>
{
    HttpClient http = new HttpClient
    {
        BaseAddress = new Uri(topdeckApiUrl),
        Timeout = TimeSpan.FromSeconds(30)
    };
    return new TCGPCardRequester.TCGPCardRequester(http);
});

// Services qui héritent d’ApiService (LeaderSheep API)
builder.Services.AddScoped<IUserService>(_ =>
{
    HttpClient http = new HttpClient
    {
        BaseAddress = new Uri(leadersheepApiUrl),
        Timeout = TimeSpan.FromSeconds(30)
    };
    return new UserService(http);
});

builder.Services.AddScoped<IDeckDetailsService>(_ =>
{
    HttpClient http = new HttpClient
    {
        BaseAddress = new Uri(leadersheepApiUrl),
        Timeout = TimeSpan.FromSeconds(30)
    };
    return new DeckDetailsService(http);
});

builder.Services.AddScoped<IDeckItemService>(_ =>
{
    HttpClient http = new HttpClient
    {
        BaseAddress = new Uri(leadersheepApiUrl),
        Timeout = TimeSpan.FromSeconds(30)
    };
    return new DeckItemService(http);
});

builder.Services.AddScoped<IVoteService>(_ =>
{
    HttpClient http = new HttpClient
    {
        BaseAddress = new Uri(leadersheepApiUrl),
        Timeout = TimeSpan.FromSeconds(30)
    };
    return new VoteService(http);
});

builder.Services.AddScoped<ITagService>(_ =>
{
    HttpClient http = new HttpClient
    {
        BaseAddress = new Uri(leadersheepApiUrl),
        Timeout = TimeSpan.FromSeconds(30)
    };
    return new TagService(http);
});



WebAssemblyHost host = builder.Build();

ILocalizer localizer = host.Services.GetRequiredService<ILocalizer>();
await localizer.InitializeAsync();

await host.RunAsync();