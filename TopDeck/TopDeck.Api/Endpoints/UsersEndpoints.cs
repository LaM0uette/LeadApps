using Microsoft.AspNetCore.Mvc;
using TopDeck.Api.Services;
using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Endpoints;

public static class UsersEndpoints
{
    #region Statements

    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/users");

        group.MapPost("oauth", GetByOAuthAsync);
        group.MapPost("uuid/{uuid:guid}", GetByUuidAsync);
        group.MapPost("", CreateAsync);
        group.MapPut("{id:int}", UpdateAsync);
        group.MapDelete("{id:int}", DeleteAsync);

        return app;
    }

    #endregion

    #region Endpoints
    
    private static async Task<IResult> GetByOAuthAsync([FromServices] IUserService service, [FromBody] AuthUserInputDTO dto, CancellationToken ct)
    {
        UserOutputDTO? item = await service.GetByOAuthIdAsync(dto, ct);
        return item is null ? Results.NotFound() : Results.Ok(item);
    }
    
    private static async Task<IResult> GetByUuidAsync([FromServices] IUserService service, Guid uuid, CancellationToken ct)
    {
        UserOutputDTO? item = await service.GetByUuidAsync(uuid, ct);
        return item is null ? Results.NotFound() : Results.Ok(item);
    }

    private static async Task<IResult> CreateAsync([FromServices] IUserService service, [FromBody] UserInputDTO dto, CancellationToken ct)
    {
        UserOutputDTO created = await service.CreateAsync(dto, ct);
        return Results.Created($"/users/{created.Id}", created);
    }

    private static async Task<IResult> UpdateAsync([FromServices] IUserService service, int id, [FromBody] UserInputDTO dto, CancellationToken ct)
    {
        UserOutputDTO? updated = await service.UpdateAsync(id, dto, ct);
        return updated is null ? Results.NotFound() : Results.Ok(updated);
    }

    private static async Task<IResult> DeleteAsync([FromServices] IUserService service, int id, CancellationToken ct)
    {
        bool ok = await service.DeleteAsync(id, ct);
        return ok ? Results.NoContent() : Results.NotFound();
    }

    #endregion
}