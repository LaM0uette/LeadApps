namespace TopDeck.Api.Entities;

public class User
{
    public int Id { get; set; }
    public required string OAuthProvider { get; set; }
    public required string OAuthId { get; set; }
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public DateTime CreatedAt { get; set; }
}