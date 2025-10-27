using Localizer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Requesters.AuthUser;
using TCGPCardRequester;
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

builder.Services.AddScoped<IAuthUserRequester, AuthUserRequester>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IDeckItemService, DeckItemService>();
builder.Services.AddScoped<IDeckDetailsService, DeckDetailsService>();
builder.Services.AddScoped<IVoteService, VoteService>();
builder.Services.AddScoped<ITagService, TagService>();

builder.Services.AddScoped<ITCGPCardRequester, TCGPCardRequester.TCGPCardRequester>();

WebAssemblyHost host = builder.Build();

ILocalizer localizer = host.Services.GetRequiredService<ILocalizer>();
await localizer.InitializeAsync();

await host.RunAsync();