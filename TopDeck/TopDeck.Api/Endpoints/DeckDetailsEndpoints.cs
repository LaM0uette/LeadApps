using Microsoft.AspNetCore.Mvc;
using TopDeck.Api.Services;
using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Endpoints;

public static class DeckDetailsEndpoints
{
    #region Statements

    public static IEndpointRouteBuilder MapDeckDetailsEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/deckDetails");

        group.MapGet("{code}", GetByCodeAsync);
        
        group.MapPost("suggestions", CreateSuggestionAsync);
        group.MapPut("suggestions/{id:int}", UpdateSuggestionAsync);
        group.MapDelete("suggestions/{id:int}", DeleteSuggestionAsync);

        return app;
    }

    #endregion

    #region Endpoints

    private static async Task<IResult> GetByCodeAsync([FromServices] IDeckDetailsService service, string code, CancellationToken ct)
    {
        DeckDetailsOutputDTO? item = await service.GetByCodeAsync(code, ct);
        return item is null ? Results.NotFound() : Results.Ok(item);
    }
    
    private static async Task<IResult> CreateSuggestionAsync([FromServices] IDeckDetailsService service, [FromBody] DeckSuggestionInputDTO dto, CancellationToken ct)
    {
        try
        {
            DeckDetailsSuggestionOutputDTO created = await service.CreateSuggestionAsync(dto, ct);
            return Results.Created($"/api/deckDetails/suggestions/{created.Id}", created);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> UpdateSuggestionAsync([FromServices] IDeckDetailsService service, int id, [FromBody] DeckSuggestionInputDTO dto, CancellationToken ct)
    {
        try
        {
            DeckDetailsSuggestionOutputDTO? updated = await service.UpdateSuggestionAsync(id, dto, ct);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> DeleteSuggestionAsync([FromServices] IDeckDetailsService service, int id, CancellationToken ct)
    {
        bool ok = await service.DeleteSuggestionAsync(id, ct);
        return ok ? Results.NoContent() : Results.NotFound();
    }

    #endregion
}
