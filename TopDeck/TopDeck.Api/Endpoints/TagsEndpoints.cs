using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TopDeck.Api.Data;
using TopDeck.Api.Entities;
using TopDeck.Api.Mappings;
using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Endpoints;

public static class TagsEndpoints
{
    public static IEndpointRouteBuilder MapTagsEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/tags");

        group.MapGet(string.Empty, GetAllAsync);

        return app;
    }

    private static async Task<IResult> GetAllAsync([FromServices] ApplicationDbContext db, CancellationToken ct)
    {
        List<Tag> tags = await db.Tags.AsNoTracking().OrderBy(t => t.Name).ToListAsync(ct);
        IEnumerable<TagOutputDTO> dtos = tags.ToOutputDTOs();
        return Results.Ok(dtos);
    }
}