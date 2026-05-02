namespace Social_Media_Chatting_APP_Domain.Entities;

public class PasswordResetToken : BaseEntity<Guid>
{
    
    /// <summary>
    /// The actual Guid you generate and send in the email link — this is what the user's URL contains
    /// </summary>
    public string Token { get; set; }
    
    
    public DateTime ExpiresAt { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// FK to AppUser — so you know whose password to reset when the token is validated
    /// </summary>
    public string UserId { get; set; }
    
    public AppUser User { get; set; }  
    /// <summary>
    /// Prevents reuse — once the password is reset, mark it true so the same link can never be used again
    /// the most important part 
    /// </summary>
    public bool IsUsed { get; set; }
    
    
    
}