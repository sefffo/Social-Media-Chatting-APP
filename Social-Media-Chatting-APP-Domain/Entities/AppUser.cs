using Microsoft.AspNetCore.Identity;

namespace Social_Media_Chatting_APP_Domain.Entities
{
    public class AppUser : IdentityUser
    {
        public string DisplayName { get; set; }
        
        public string ProfilePicture { get; set; }
        
        public string BIO { get; set; }
        
        public bool IsTwoFactorSetup { get; set; }
        
        public bool IsGoogleAccount  { get; set; }
        
        public bool isOnline { get; set; }
        
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? LastSeen  { get; set; }
        
    }
}
