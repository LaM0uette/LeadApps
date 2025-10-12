using Microsoft.AspNetCore.Mvc;
using TopDeck.Api.Services;
using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Endpoints;

public static class DeckDetailsEndpoints
{
    public static IEndpointRouteBuilder MapDeckDetailsEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/deckDetails");

        group.MapGet("{code}", GetByCodeAsync);

        return app;
    }

    private static async Task<IResult> GetByCodeAsync([FromServices] IDeckDetailsService service, string code, CancellationToken ct)
    {
        DeckDetailsOutputDTO? item = await service.GetByCodeAsync(code, ct);
        return item is null ? Results.NotFound() : Results.Ok(item);
    }
}
