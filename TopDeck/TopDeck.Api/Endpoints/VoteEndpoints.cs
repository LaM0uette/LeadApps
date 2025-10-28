using Microsoft.AspNetCore.Mvc;
using TopDeck.Api.Services;
using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Endpoints;

public static class VoteEndpoints
{
    #region Statements

    public static IEndpointRouteBuilder MapVoteEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/vote");

        group.MapPost("deck", VoteDeckAsync);
        group.MapPost("deckSuggestion", VoteDeckSuggestionAsync);

        return app;
    }

    #endregion

    #region Endpoints

    private static async Task<IResult> VoteDeckAsync([FromServices] IVoteService service, [FromBody] DeckVoteInputDTO dto, CancellationToken ct)
    {
        if (dto.Id == -1 || string.IsNullOrWhiteSpace(dto.UserUuid))
            return Results.Unauthorized();
        
        try
        {
            bool succes = await service.VoteDeckAsync(dto, ct);
            return Results.Ok(succes);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }
    
    private static async Task<IResult> VoteDeckSuggestionAsync([FromServices] IVoteService service, [FromBody] DeckSuggestionVoteInputDTO dto, CancellationToken ct)
    {
        if (dto.Id == -1 || string.IsNullOrWhiteSpace(dto.UserUuid))
            return Results.Unauthorized();
        
        try
        {
            bool success = await service.VoteDeckSuggestionAsync(dto, ct);
            return Results.Ok(success);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    #endregion
}
