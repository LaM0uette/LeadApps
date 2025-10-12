using TopDeck.Api.Entities;
using TopDeck.Contracts.DTO;

namespace TopDeck.Api.Mappings;

public static class UserMappings
{
    public static UserOutputDTO MapToDTO(this User user)
    {
        return new UserOutputDTO(
            user.Id,
            user.OAuthProvider,
            user.Uuid.ToString(),
            user.UserName,
            user.CreatedAt
        );
    }
    
    public static User ToEntity(this UserInputDTO dto)
    {
        return new User
        {
            Uuid = Guid.NewGuid(),
            OAuthProvider = dto.OAuthProvider,
            OAuthId = dto.OAuthId,
            UserName = dto.UserName
        };
    }
    
    public static void UpdateEntity(this User entity, UserInputDTO dto)
    {
        entity.UserName = dto.UserName;
    }
}