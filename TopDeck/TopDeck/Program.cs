using System.Security.Claims;
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using TopDeck.Components;
using TopDeck.Contracts.DTO;
using TopDeck.Endpoints;
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

                string? authId = user?.FindFirstValue(ClaimTypes.NameIdentifier);
                string provider = authId?.Split('|').FirstOrDefault() ?? "unknown";

                string? fullName = user?.FindFirstValue(ClaimTypes.Name);
                string? given    = user?.FindFirstValue(ClaimTypes.GivenName);
                string? surname  = user?.FindFirstValue(ClaimTypes.Surname);
                string email    = user?.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

                string userName = !string.IsNullOrWhiteSpace(fullName)
                    ? fullName
                    : $"{given} {surname}".Trim();

                if (!string.IsNullOrEmpty(authId))
                {
                    IHttpClientFactory factory = context.HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>();
                    HttpClient http = factory.CreateClient("Api");

                    UserInputDTO dto = new(
                        provider,
                        authId,
                        userName,
                        email
                    );

                    await http.PostAsJsonAsync("api/users", dto, context.HttpContext.RequestAborted);
                }
            }
        };
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