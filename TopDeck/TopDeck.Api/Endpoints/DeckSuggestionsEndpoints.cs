using Microsoft.AspNetCore.Mvc;
using TopDeck.Api.Services.Interfaces;
using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Endpoints;

public static class DeckSuggestionsEndpoints
{
    #region Statements

    public static IEndpointRouteBuilder MapDeckSuggestionsEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/deck-suggestions");

        group.MapGet("", GetAllAsync);
        group.MapGet("{id:int}", GetByIdAsync);
        group.MapPost("", CreateAsync);
        group.MapPut("{id:int}", UpdateAsync);
        group.MapDelete("{id:int}", DeleteAsync);

        return app;
    }

    #endregion

    #region Endpoints

    private static async Task<IResult> GetAllAsync([FromServices] IDeckSuggestionService service, CancellationToken ct)
    {
        IReadOnlyList<DeckSuggestionOutputDTO> items = await service.GetAllAsync(ct);
        return Results.Ok(items);
    }

    private static async Task<IResult> GetByIdAsync([FromServices] IDeckSuggestionService service, int id, CancellationToken ct)
    {
        DeckSuggestionOutputDTO? item = await service.GetByIdAsync(id, ct);
        return item is null ? Results.NotFound() : Results.Ok(item);
    }

    private static async Task<IResult> CreateAsync([FromServices] IDeckSuggestionService service, [FromBody] DeckSuggestionInputDTO dto, CancellationToken ct)
    {
        try
        {
            DeckSuggestionOutputDTO created = await service.CreateAsync(dto, ct);
            return Results.Created($"/api/deck-suggestions/{created.Id}", created);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> UpdateAsync([FromServices] IDeckSuggestionService service, int id, [FromBody] DeckSuggestionInputDTO dto, CancellationToken ct)
    {
        try
        {
            DeckSuggestionOutputDTO? updated = await service.UpdateAsync(id, dto, ct);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> DeleteAsync([FromServices] IDeckSuggestionService service, int id, CancellationToken ct)
    {
        bool ok = await service.DeleteAsync(id, ct);
        return ok ? Results.NoContent() : Results.NotFound();
    }

    #endregion
}