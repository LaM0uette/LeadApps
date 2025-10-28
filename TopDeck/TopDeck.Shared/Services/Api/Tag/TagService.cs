using TopDeck.Contracts.DTO;
using TopDeck.Domain.Models;
using TopDeck.Shared.Mappings;

namespace TopDeck.Shared.Services;

public class TagService : ApiService, ITagService
{
    private const string _route = "/tags";
    
    public TagService(HttpClient http) : base(http) { }

    public async Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken ct = default)
    {
        IReadOnlyList<TagOutputDTO>? dtos = await GetJsonAsync<IReadOnlyList<TagOutputDTO>>(_route, ct);
        return dtos?.ToDomain() ?? [];
    }
}