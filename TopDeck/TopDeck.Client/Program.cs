using Localizer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Requesters.AuthUser;
using TCGPCardRequester;
using TopDeck.Shared.Services;
using TopDeck.Shared.UIStore;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

// HttpClient par défaut (assets Blazor)
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
    "Preproduction" => "https://api.topdeck.preprod.tehleadersheep.com",
    "Production"    => "https://api.topdeck.tehleadersheep.com",
    _ => throw new Exception($"Environnement inconnu : {builder.HostEnvironment.Environment}")
};

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();
builder.Services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();

// Stores et localizer
builder.Services.AddSingleton<UIStore>();
builder.Services.AddScoped<ILocalizer, JsonLocalizer>();

// AuthUserRequester (si besoin d’un HttpClient dédié, tu peux aussi le mettre en AddHttpClient)
builder.Services.AddScoped<IAuthUserRequester, AuthUserRequester>();

// TCGPCardRequester (TopDeck API)
builder.Services.AddHttpClient<ITCGPCardRequester, TCGPCardRequester.TCGPCardRequester>(client =>
{
    client.BaseAddress = new Uri(topdeckApiUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Services LeaderSheep API
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

WebAssemblyHost host = builder.Build();

ILocalizer localizer = host.Services.GetRequiredService<ILocalizer>();
await localizer.InitializeAsync();

await host.RunAsync();
