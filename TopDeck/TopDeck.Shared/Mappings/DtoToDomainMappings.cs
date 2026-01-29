using TopDeck.Contracts.DTO;
using TopDeck.Domain.Models;
using TopDeck.Shared.Models.TCGP;

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

    // ===== TCGP Mappings =====
    public static TCGPCardType ToDomain(this CardTypeOutputDTO dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));
        return new TCGPCardType(dto.Id, dto.Name);
    }

    public static TCGPCardCollection ToDomain(this CardCollectionOutputDTO dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));
        return new TCGPCardCollection(dto.Code);
    }

    public static TCGPPokemonType ToDomain(this PokemonTypeOutputDTO dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));
        return new TCGPPokemonType(dto.Id, dto.Name);
    }

    public static TCGPPokemonSpecial ToDomain(this PokemonSpecialOutputDTO dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));
        return new TCGPPokemonSpecial(dto.Id, dto.Name);
    }

    public static TCGPCard ToDomain(this CardOutputDTO dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));
        return new TCGPCard(
            dto.Type.ToDomain(),
            dto.Name,
            dto.ImageUrl,
            dto.Collection.ToDomain(),
            dto.CollectionNumber
        );
    }

    public static TCGPCard ToDomain(this CardItemOutputDTO dto) => ((CardOutputDTO)dto).ToDomain();
    public static TCGPCard ToDomain(this CardToolOutputDTO dto) => ((CardOutputDTO)dto).ToDomain();
    public static TCGPCard ToDomain(this CardSupporterOutputDTO dto) => ((CardOutputDTO)dto).ToDomain();
    public static TCGPCard ToDomain(this CardStadiumOutputDTO dto) => ((CardOutputDTO)dto).ToDomain();
    public static TCGPCard ToDomain(this CardFossilOutputDTO dto) => ((CardOutputDTO)dto).ToDomain();

    public static TCGPPokemonCard ToDomain(this CardPokemonOutputDTO dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));
        return new TCGPPokemonCard(
            dto.Type.ToDomain(),
            dto.Name,
            dto.ImageUrl,
            dto.Collection.ToDomain(),
            dto.CollectionNumber,
            dto.PokemonSpecials.Select(s => s.ToDomain()).ToList(),
            dto.PokemonType.ToDomain()
        );
    }

    public static List<TCGPCard> ToDomain(this IEnumerable<CardItemOutputDTO> dtos) => dtos.Select(d => d.ToDomain()).ToList();
    public static List<TCGPCard> ToDomain(this IEnumerable<CardToolOutputDTO> dtos) => dtos.Select(d => d.ToDomain()).ToList();
    public static List<TCGPCard> ToDomain(this IEnumerable<CardSupporterOutputDTO> dtos) => dtos.Select(d => d.ToDomain()).ToList();
    public static List<TCGPCard> ToDomain(this IEnumerable<CardStadiumOutputDTO> dtos) => dtos.Select(d => d.ToDomain()).ToList();
    public static List<TCGPCard> ToDomain(this IEnumerable<CardFossilOutputDTO> dtos) => dtos.Select(d => d.ToDomain()).ToList();
    public static List<TCGPPokemonCard> ToDomain(this IEnumerable<CardPokemonOutputDTO> dtos) => dtos.Select(d => d.ToDomain()).ToList();
}
