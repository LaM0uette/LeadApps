using Microsoft.AspNetCore.Mvc;
using TopDeck.Api.Services;
using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Endpoints;

public static class UsersEndpoints
{
    #region Statements

    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/users");

        group.MapGet("", GetAllAsync);
        group.MapGet("{id:int}", GetByIdAsync);
        group.MapPost("oauth", GetByOAuthAsync);
        group.MapPost("", CreateAsync);
        group.MapPut("{id:int}", UpdateAsync);
        group.MapDelete("{id:int}", DeleteAsync);

        return app;
    }

    #endregion

    #region Endpoints

    private static async Task<IResult> GetAllAsync([FromServices] IUserService service, CancellationToken ct)
    {
        IReadOnlyList<UserOutputDTO> items = await service.GetAllAsync(ct);
        return Results.Ok(items);
    }

    private static async Task<IResult> GetByIdAsync([FromServices] IUserService service, int id, CancellationToken ct)
    {
        UserOutputDTO? item = await service.GetByIdAsync(id, ct);
        return item is null ? Results.NotFound() : Results.Ok(item);
    }
    
    private static async Task<IResult> GetByOAuthAsync([FromServices] IUserService service, [FromBody] UserOAuthInputDTO dto, CancellationToken ct)
    {
        UserOutputDTO? item = await service.GetByOAuthAsync(dto, ct);
        return item is null ? Results.NotFound() : Results.Ok(item);
    }

    private static async Task<IResult> CreateAsync([FromServices] IUserService service, [FromBody] UserInputDTO dto, CancellationToken ct)
    {
        UserOutputDTO created = await service.CreateAsync(dto, ct);
        return Results.Created($"/api/users/{created.Id}", created);
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