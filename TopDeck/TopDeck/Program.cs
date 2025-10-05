using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using TopDeck.Components;
using TopDeck.Endpoints;
using TopDeck.Shared.UIStore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuth0WebAppAuthentication(options => {
        options.Domain = builder.Configuration["Auth0:Domain"];
        options.ClientId = builder.Configuration["Auth0:ClientId"];
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

// Services
builder.Services.AddSingleton<UIStore>();

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

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapAuthEndpoints();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(TopDeck.Client._Imports).Assembly);

app.Run();