using System.Text.Json.Serialization;

namespace TopDeck.Contracts.DTO;

public record UserOutputDTO(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("oAuthProvider")] string OAuthProvider,
    [property: JsonPropertyName("oAuthId")] string OAuthId,
    [property: JsonPropertyName("userName")] string UserName,
    [property: JsonPropertyName("createdAt")] DateTime CreatedAt
);