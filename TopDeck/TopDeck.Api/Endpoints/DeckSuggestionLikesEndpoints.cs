using Microsoft.AspNetCore.Mvc;
using TopDeck.Api.Services.Interfaces;
using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Endpoints;

public static class DeckSuggestionLikesEndpoints
{
    #region Statements

    public static IEndpointRouteBuilder MapDeckSuggestionLikesEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/deck-suggestion-likes");

        group.MapPost("", CreateAsync);
        group.MapDelete("{deckSuggestionId:int}/{userId:int}", DeleteAsync);

        return app;
    }

    #endregion

    #region Endpoints

    private static async Task<IResult> CreateAsync([FromServices] IDeckSuggestionLikeService service, [FromBody] DeckSuggestionLikeInputDTO dto, CancellationToken ct)
    {
        try
        {
            DeckSuggestionLikeOutputDTO? created = await service.CreateAsync(dto, ct);
            return Results.Created($"/api/deck-suggestion-likes", created);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> DeleteAsync([FromServices] IDeckSuggestionLikeService service, int deckSuggestionId, int userId, CancellationToken ct)
    {
        bool ok = await service.DeleteAsync(deckSuggestionId, userId, ct);
        return ok ? Results.NoContent() : Results.NotFound();
    }

    #endregion
}
