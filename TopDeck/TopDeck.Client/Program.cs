using Localizer;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TopDeck.Shared.Services;
using TopDeck.Shared.UIStore;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

// Services
builder.Services.AddSingleton<UIStore>();
builder.Services.AddScoped<ILocalizer, JsonLocalizer>();

builder.Services.AddScoped<IDeckService, DeckService>();

WebAssemblyHost host = builder.Build();

ILocalizer localizer = host.Services.GetRequiredService<ILocalizer>();
await localizer.InitializeAsync();

await host.RunAsync();