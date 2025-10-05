using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace TopDeck.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/Account");

        group.MapGet("/Login", LoginAuthenticationPropertiesBuilder);
        group.MapGet("/Logout", Logout);

        return app;
    }

    private static async Task LoginAuthenticationPropertiesBuilder(HttpContext httpContext, string returnUrl = "/")
    {
        AuthenticationProperties authenticationProperties = new LoginAuthenticationPropertiesBuilder()
            .WithRedirectUri(returnUrl)
            .Build();

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