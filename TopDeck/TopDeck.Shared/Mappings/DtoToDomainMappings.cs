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
}
