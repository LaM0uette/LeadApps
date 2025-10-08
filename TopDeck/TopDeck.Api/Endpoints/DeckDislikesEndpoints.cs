using Microsoft.AspNetCore.Mvc;
using TopDeck.Api.Services.Interfaces;
using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Endpoints;

public static class DeckDislikesEndpoints
{
    #region Statements

    public static IEndpointRouteBuilder MapDeckDislikesEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/deck-dislikes");

        group.MapPost("", CreateAsync);
        group.MapDelete("{deckId:int}/{userId:int}", DeleteAsync);

        return app;
    }

    #endregion

    #region Endpoints

    private static async Task<IResult> CreateAsync([FromServices] IDeckDislikeService service, [FromBody] DeckDislikeInputDTO dto, CancellationToken ct)
    {
        try
        {
            DeckDislikeOutputDTO? created = await service.CreateAsync(dto, ct);
            return Results.Created($"/api/deck-dislikes", created);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> DeleteAsync([FromServices] IDeckDislikeService service, int deckId, int userId, CancellationToken ct)
    {
        bool ok = await service.DeleteAsync(deckId, userId, ct);
        return ok ? Results.NoContent() : Results.NotFound();
    }

    #endregion
}
