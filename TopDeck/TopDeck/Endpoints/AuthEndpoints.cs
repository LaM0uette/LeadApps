using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace TopDeck.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/Account");

        group.MapGet("/Login", Login);
        group.MapGet("/Logout", Logout);

        return app;
    }

    private static async Task Login(HttpContext httpContext, string returnUrl = "/", string? provider = null)
    {
        LoginAuthenticationPropertiesBuilder builder = new LoginAuthenticationPropertiesBuilder().WithRedirectUri(returnUrl);

        if (!string.IsNullOrEmpty(provider))
        {
            builder.WithParameter("connection", provider);
        }

        AuthenticationProperties authenticationProperties = builder.Build();
        await httpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
    }

    private static async Task Logout(HttpContext httpContext)
    {
        AuthenticationProperties authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
            .WithRedirectUri("/")
            .Build();

        await httpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}