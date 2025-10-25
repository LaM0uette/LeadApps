using TopDeck.Api.Entities;
using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Mappings;

public static class TagMappings
{
    public static TagOutputDTO ToOutputDTO(this Tag entity)
    {
        return new TagOutputDTO(entity.Id, entity.Name, entity.ColorHex);
    }

    public static IEnumerable<TagOutputDTO> ToOutputDTOs(this IEnumerable<Tag> entities)
    {
        return entities.Select(e => e.ToOutputDTO());
    }
}