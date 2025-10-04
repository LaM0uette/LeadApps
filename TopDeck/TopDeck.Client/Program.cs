using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TopDeck.Shared.UIStore;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

// Services
builder.Services.AddSingleton<UIStore>();

await builder.Build().RunAsync();