using TopDeck.Api.Entities;
using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Mappings;

public static class UserMappings
{
    public static User ToEntity(this UserInputDTO dto)
    {
        return new User
        {
            OAuthProvider = dto.OAuthProvider,
            OAuthId = dto.OAuthId,
            UserName = dto.UserName,
            Email = dto.Email
        };
    }

    public static void UpdateEntity(this User entity, UserInputDTO dto)
    {
        entity.OAuthProvider = dto.OAuthProvider;
        entity.OAuthId = dto.OAuthId;
        entity.UserName = dto.UserName;
        entity.Email = dto.Email;
    }

    public static UserOutputDTO ToOutput(this User entity)
    {
        return new UserOutputDTO(entity.Id, entity.OAuthProvider, entity.OAuthId, entity.UserName, entity.Email, entity.CreatedAt);
    }

    public static UserInputDTO ToInput(this User entity)
    {
        return new UserInputDTO(entity.OAuthProvider, entity.OAuthId, entity.UserName, entity.Email);
    }
}