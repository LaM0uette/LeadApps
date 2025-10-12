using Microsoft.AspNetCore.Mvc;
using TopDeck.Api.Services;
using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Endpoints;

public static class DeckLikesEndpoints
{
    #region Statements

    public static IEndpointRouteBuilder MapDeckLikesEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/deck-likes");

        group.MapPost("", CreateAsync);
        group.MapDelete("{deckId:int}/{userId:int}", DeleteAsync);

        return app;
    }

    #endregion

    #region Endpoints

    private static async Task<IResult> CreateAsync([FromServices] IDeckLikeService service, [FromBody] DeckLikeInputDTO dto, CancellationToken ct)
    {
        try
        {
            DeckLikeOutputDTO? created = await service.CreateAsync(dto, ct);
            return Results.Created($"/api/deck-likes", created);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> DeleteAsync([FromServices] IDeckLikeService service, int deckId, int userId, CancellationToken ct)
    {
        bool ok = await service.DeleteAsync(deckId, userId, ct);
        return ok ? Results.NoContent() : Results.NotFound();
    }

    #endregion
}
