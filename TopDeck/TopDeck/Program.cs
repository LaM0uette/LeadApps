using System.Globalization;
using System.Security.Claims;
using Auth0.AspNetCore.Authentication;
using Helpers.Auth0;
using Localizer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;
using Requesters.AuthUser;
using TopDeck.Components;
using TopDeck.Contracts.DTO;
using TopDeck.Endpoints;
using TopDeck.FakeServices;
using TopDeck.Shared.Services;
using TopDeck.Shared.UIStore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]);
});

builder.Services
    .AddAuth0WebAppAuthentication(options => {
        options.Domain = builder.Configuration["Auth0:Domain"];
        options.ClientId = builder.Configuration["Auth0:ClientId"];
        options.OpenIdConnectEvents = new OpenIdConnectEvents
        {
            OnTokenValidated = async context =>
            {
                ClaimsPrincipal? user = context.Principal;
                string? sub = user?.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (!Auth0SubHelper.TryParse(sub, out string provider, out string authId)) 
                    return;

                string? fullName = user?.FindFirstValue(ClaimTypes.Name);
                string? given = user?.FindFirstValue(ClaimTypes.GivenName);
                string? surname = user?.FindFirstValue(ClaimTypes.Surname);
                string email = user?.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
                string userName = !string.IsNullOrWhiteSpace(fullName) ? fullName : $"{given} {surname}".Trim();

                IHttpClientFactory factory = context.HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>();
                HttpClient http = factory.CreateClient("Api");

                UserInputDTO dto = new(provider, authId, userName, email);
                await http.PostAsJsonAsync("api/users", dto, context.HttpContext.RequestAborted);
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

builder.Services.AddSingleton<IDeckService, FakeDeckService>();
builder.Services.AddScoped<IUserService, FakeUserService>();
builder.Services.AddScoped<IAuthUserRequester, FakeAuthUserRequester>();

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