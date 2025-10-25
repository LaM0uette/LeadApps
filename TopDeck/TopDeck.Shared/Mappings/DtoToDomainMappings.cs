using TopDeck.Contracts.DTO;
using TopDeck.Domain.Models;

namespace TopDeck.Shared.Mappings;

/// <summary>
/// Mapping helpers to convert API OutputDTOs to Domain models.
/// Note: Only DTOs and domain models currently available are mapped.
/// </summary>
public static class DtoToDomainMappings
{
    // USER
    public static User ToDomain(this UserOutputDTO dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));
        return new User(
            dto.Id,
            dto.OAuthProvider,
            dto.Uuid,
            dto.UserName,
            dto.CreatedAt
        );
    }

    // TAG
    public static Tag ToDomain(this TagOutputDTO dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));
        return new Tag(dto.Id, dto.Name, dto.ColorHex);
    }

    public static IReadOnlyList<Tag> ToDomain(this IEnumerable<TagOutputDTO> dtos)
    {
        return dtos.Select(d => d.ToDomain()).ToList();
    }
}
