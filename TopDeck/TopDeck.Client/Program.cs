using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TopDeck.Shared.UIStore;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

// Services
builder.Services.AddSingleton<UIStore>();

await builder.Build().RunAsync();