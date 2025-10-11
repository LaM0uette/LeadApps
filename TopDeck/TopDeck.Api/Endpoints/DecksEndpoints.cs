using Microsoft.AspNetCore.Mvc;
using TopDeck.Api.DTO;
using TopDeck.Api.Services.Interfaces;
using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Endpoints;

public static class DecksEndpoints
{
    #region Statements

    public static IEndpointRouteBuilder MapDecksEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/decks");

        group.MapGet("", GetAllAsync);
        group.MapGet("{id:int}", GetByIdAsync);
        group.MapPost("", CreateAsync);
        group.MapPut("{id:int}", UpdateAsync);
        group.MapDelete("{id:int}", DeleteAsync);
        
        group.MapGet("page", GetPageAsync);
        group.MapGet("deck/{code}", GetByCodeAsync);

        return app;
    }

    #endregion

    #region Endpoints

    private static async Task<IResult> GetAllAsync([FromServices] IDeckService service, bool includeAll, CancellationToken ct)
    {
        IReadOnlyList<DeckOutputDTO> items = await service.GetAllAsync(includeAll, ct);
        return Results.Ok(items);
    }
    
    private static async Task<IResult> GetByIdAsync([FromServices] IDeckService service, int id, bool includeAll, CancellationToken ct)
    {
        DeckOutputDTO? item = await service.GetByIdAsync(id, includeAll, ct);
        return item is null ? Results.NotFound() : Results.Ok(item);
    }

    private static async Task<IResult> CreateAsync([FromServices] IDeckService service, [FromBody] DeckInputDTO dto, CancellationToken ct)
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

    private static async Task<IResult> UpdateAsync([FromServices] IDeckService service, int id, [FromBody] DeckInputDTO dto, CancellationToken ct)
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

    private static async Task<IResult> DeleteAsync([FromServices] IDeckService service, int id, CancellationToken ct)
    {
        bool ok = await service.DeleteAsync(id, ct);
        return ok ? Results.NoContent() : Results.NotFound();
    }
    
    
    private static async Task<IResult> GetPageAsync([FromServices] IDeckService service, [FromQuery] int skip = 0, [FromQuery] int take = 20, CancellationToken ct = default)
    {
        if (take <= 0) 
            take = 20;
        
        if (skip < 0) 
            skip = 0;
        
        IReadOnlyList<DeckOutputDTO> items = await service.GetDeckCardPageAsync(skip, take, ct);
        return Results.Ok(items);
    }
    
    private static async Task<IResult> GetByCodeAsync([FromServices] IDeckService service, string code, CancellationToken ct)
    {
        DeckOutputDTO? item = await service.GetDeckCardByCodeAsync(code, ct);
        return item is null ? Results.NotFound() : Results.Ok(item);
    }

    #endregion
}