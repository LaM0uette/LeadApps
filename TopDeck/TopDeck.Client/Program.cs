using Localizer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Requesters.AuthUser;
using TCGPocketDex.SDK.Http;
using TCGPocketDex.SDK.Services;
using TopDeck.Shared.Services;
using TopDeck.Shared.UIStore;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();
builder.Services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();

// Services
builder.Services.AddSingleton<UIStore>();
builder.Services.AddScoped<ILocalizer, JsonLocalizer>();

builder.Services.AddScoped<IDeckService, DeckService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthUserRequester, AuthUserRequester>();
builder.Services.AddScoped<IDeckReactionService, DeckReactionService>();

builder.Services.AddScoped<IApiClient, ApiClient>();
builder.Services.AddScoped<ICardService, CardService>();

WebAssemblyHost host = builder.Build();

ILocalizer localizer = host.Services.GetRequiredService<ILocalizer>();
await localizer.InitializeAsync();

await host.RunAsync();