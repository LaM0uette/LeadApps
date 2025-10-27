using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using TopDeck.Api.Services;
using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Endpoints;

public static class DeckItemEndpoints
{
    #region Statements

    public static IEndpointRouteBuilder MapDeckItemEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/deckItems");

        group.MapPost("page", GetPageAsync);
        group.MapGet("count", GetCountAsync);
        group.MapGet("deckItem/{code}", GetByCodeAsync);
        group.MapPost("", CreateAsync);
        group.MapPut("{id:int}", UpdateAsync);
        group.MapDelete("{id:int}", DeleteAsync);
        
        return app;
    }

    #endregion

    #region Endpoints
    
    private static async Task<IResult> GetPageAsync(
        [FromServices] IDeckItemService service,
        [FromBody] DeckItemsFilterDTO filter,
        CancellationToken ct = default)
    {
        var safeFilter = new DeckItemsFilterDTO
        {
            Skip = filter.Skip < 0 ? 0 : filter.Skip,
            Take = filter.Take <= 0 ? 20 : filter.Take,
            Search = filter.Search,
            TagIds = filter.TagIds,
            OrderBy = filter.OrderBy,
            Asc = filter.Asc
        };
        IReadOnlyList<DeckItemOutputDTO> items = await service.GetPageAsync(safeFilter, ct);
        return Results.Ok(items);
    }
    
    private static async Task<IResult> GetByCodeAsync([FromServices] IDeckItemService service, string code, CancellationToken ct)
    {
        DeckItemOutputDTO? item = await service.GetByCodeAsync(code, ct);
        return item is null ? Results.NotFound() : Results.Ok(item);
    }

    private static async Task<IResult> CreateAsync([FromServices] IDeckItemService service, [FromBody] DeckItemInputDTO dto, CancellationToken ct)
    {
        try
        {
            DeckItemOutputDTO created = await service.CreateAsync(dto, ct);
            return Results.Created($"/api/decks/{created.Id}", created);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> UpdateAsync([FromServices] IDeckItemService service, int id, [FromBody] DeckItemInputDTO dto, CancellationToken ct)
    {
        try
        {
            DeckItemOutputDTO? updated = await service.UpdateAsync(id, dto, ct);
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
    
    private static async Task<IResult> GetCountAsync(
        [FromServices] IDeckItemService service,
        [FromQuery] string? search = null,
        [FromQuery] int[]? tagIds = null,
        CancellationToken ct = default)
    {
        var filter = new DeckItemsFilterDTO
        {
            Skip = 0,
            Take = 0,
            Search = search,
            TagIds = tagIds,
            OrderBy = null,
            Asc = false
        };
        int count = await service.GetTotalCountAsync(filter, ct);
        return Results.Ok(count);
    }
    
    #endregion
}