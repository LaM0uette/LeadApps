using System.ComponentModel.DataAnnotations;

namespace TopDeck.Api.Entities;

public class User
{
    public int Id { get; set; }

    public Guid Uuid { get; set; }
    
    [MaxLength(50)]
    public required string OAuthProvider { get; set; }
    
    [MaxLength(100)]
    public required string OAuthId { get; set; }
    
    [MaxLength(20)]
    public required string UserName { get; set; }
    
    public DateTime CreatedAt { get; set; }
}