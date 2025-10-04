using System.ComponentModel.DataAnnotations;

namespace TopDeck.Api.Entities;

public class User
{
    public int Id { get; set; }
    
    [MaxLength(50)]
    public required string OAuthProvider { get; set; }
    
    [MaxLength(100)]
    public required string OAuthId { get; set; }
    
    [MaxLength(100)]
    public required string UserName { get; set; }
    
    [MaxLength(200)]
    public required string Email { get; set; }
    
    public DateTime CreatedAt { get; set; }
}