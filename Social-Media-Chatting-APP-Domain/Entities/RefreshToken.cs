namespace Social_Media_Chatting_APP_Domain.Entities;

public class RefreshToken : BaseEntity<Guid> 
{
    public string TokenHash { get; set; } = default!;
    
    /// <summary>
    /// (jti Claim) used to be assigned to only one Access Token
    /// and stored in the DB and compare the expired access token with the stored row in the DB
    /// and if it matches it this means that's a valid user , 
    /// and then generate a long refresh token for the user  
    /// </summary>
    public string JwtId { get; set; } = default!;
    
    public DateTime ExpiresAt { get; set; }

    public string UserId { set; get; } = default!;
    
    // nav prop
    public AppUser User { set; get; }
    
    
    public DateTime CreatedAt { set; get; }
    public DateTime UpdatedAt { set; get; }
    
    
    /// <summary>
    /// used for checking if this refresh token used before
    /// A non-null value means the token was already used once (rotation).
    /// </summary>
    public DateTime? UsedAt { set; get; }
    
    /// <summary>
    /// Set when this token is explicitly revoked (logout, admin action)
    /// </summary>
    public DateTime? RevokedAt { get; set; }
    
    /// <summary>
    /// True only when the token is safe to use:
    ///   - Has never been used (UsedAt is null)
    ///   - Has never been revoked (RevokedAt is null)
    ///   - Has not expired (current time is before ExpiresAt)
    /// All three conditions must be true simultaneously.
    /// </summary>
    public bool IsActive =>
        UsedAt is null &&
        RevokedAt is null &&
        DateTime.UtcNow < ExpiresAt;



}