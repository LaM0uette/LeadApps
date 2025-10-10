using Microsoft.AspNetCore.Mvc;
using TopDeck.Api.Services.Interfaces;
using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Endpoints;

public static class DeckSuggestionDislikesEndpoints
{
    #region Statements

    public static IEndpointRouteBuilder MapDeckSuggestionDislikesEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/deck-suggestion-dislikes");

        group.MapPost("", CreateAsync);
        group.MapDelete("{deckSuggestionId:int}/{userId:int}", DeleteAsync);

        return app;
    }

    #endregion

    #region Endpoints

    private static async Task<IResult> CreateAsync([FromServices] IDeckSuggestionDislikeService service, [FromBody] DeckSuggestionDislikeInputDTO dto, CancellationToken ct)
    {
        try
        {
            DeckSuggestionDislikeOutputDTO? created = await service.CreateAsync(dto, ct);
            return Results.Created($"/api/deck-suggestion-dislikes", created);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> DeleteAsync([FromServices] IDeckSuggestionDislikeService service, int deckSuggestionId, int userId, CancellationToken ct)
    {
        bool ok = await service.DeleteAsync(deckSuggestionId, userId, ct);
        return ok ? Results.NoContent() : Results.NotFound();
    }

    #endregion
}
