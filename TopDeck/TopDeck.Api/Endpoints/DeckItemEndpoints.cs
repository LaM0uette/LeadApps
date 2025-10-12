using Microsoft.AspNetCore.Mvc;
using TopDeck.Api.DTO;
using TopDeck.Api.Services.Interfaces;
using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Endpoints;

public static class DeckItemEndpoints
{
    #region Statements

    public static IEndpointRouteBuilder MapDeckItemEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/deckItems");

        group.MapGet("page", GetPageAsync);
        group.MapGet("deckItem/{code}", GetByCodeAsync);
        group.MapPost("", CreateAsync);
        group.MapPut("{id:int}", UpdateAsync);
        group.MapDelete("{id:int}", DeleteAsync);
        
        return app;
    }

    #endregion

    #region Endpoints
    
    private static async Task<IResult> GetPageAsync([FromServices] IDeckItemService service, [FromQuery] int skip = 0, [FromQuery] int take = 20, CancellationToken ct = default)
    {
        if (take <= 0) 
            take = 20;
        
        if (skip < 0) 
            skip = 0;
        
        IReadOnlyList<DeckOutputDTO> items = await service.GetDeckCardPageAsync(skip, take, ct);
        return Results.Ok(items);
    }
    
    private static async Task<IResult> GetByCodeAsync([FromServices] IDeckItemService service, string code, CancellationToken ct)
    {
        DeckOutputDTO? item = await service.GetDeckCardByCodeAsync(code, ct);
        return item is null ? Results.NotFound() : Results.Ok(item);
    }

    private static async Task<IResult> CreateAsync([FromServices] IDeckItemService service, [FromBody] DeckInputDTO dto, CancellationToken ct)
    {
        try
        {
            DeckOutputDTO created = await service.CreateAsync(dto, ct);
            return Results.Created($"/api/decks/{created.Id}", created);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> UpdateAsync([FromServices] IDeckItemService service, int id, [FromBody] DeckInputDTO dto, CancellationToken ct)
    {
        try
        {
            DeckOutputDTO? updated = await service.UpdateAsync(id, dto, ct);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> DeleteAsync([FromServices] IDeckItemService service, int id, CancellationToken ct)
    {
        bool ok = await service.DeleteAsync(id, ct);
        return ok ? Results.NoContent() : Results.NotFound();
    }
    
    #endregion
}